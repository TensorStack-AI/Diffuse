using DiffuseApp.Common.Config;
using DiffuseApp.Common.Message;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Tensor;
using TensorStack.Python.Common;
using TensorStack.Python.Config;

namespace DiffuseApp.Common
{
    public sealed class PipelineClient : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ClientConfig _config;
        private readonly NamedPipeClientStream _commandChannel;
        private readonly NamedPipeClientStream _pipelineChannel;
        private readonly NamedPipeClientStream _progressChannel;
        private readonly ProcessLifetimeHandler _processHandler;
        private readonly IProgress<PipelineProgress> _progressCallback;
        private readonly CancellationTokenSource _progressCancellation;
        private Process _serverProcess;
        private bool _isCanceled;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineClient"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <param name="logger">The logger.</param>
        public PipelineClient(ClientConfig config, IProgress<PipelineProgress> progressCallback, ILogger logger = default)
        {
            _logger = logger;
            _config = config;
            _progressCallback = progressCallback;
            _processHandler = new ProcessLifetimeHandler();
            _progressCancellation = new CancellationTokenSource();
            _commandChannel = new NamedPipeClientStream(".", ServerConfig.ChannelCommand, PipeDirection.InOut, PipeOptions.Asynchronous);
            _pipelineChannel = new NamedPipeClientStream(".", ServerConfig.ChannelPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _progressChannel = new NamedPipeClientStream(".", ServerConfig.ChannelProgress, PipeDirection.In, PipeOptions.Asynchronous);
            _ = ProcessProgressQueueAsync(_progressCallback);
        }


        /// <summary>
        /// Start client as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _isCanceled = false;

            // Start Server
            await StartServerAsync();

            try
            {
                // Connect Pipes
                await Task.WhenAll
                (
                    _commandChannel.ConnectAsync(cancellationToken),
                    _progressChannel.ConnectAsync(cancellationToken),
                    _pipelineChannel.ConnectAsync(cancellationToken)
                );

                // Start Environment
                await SendPipelineRequestAsync(new PipelineRequest(RequestType.Start), cancellationToken);
                await SendPipelineRequestAsync(new PipelineRequest(_config.Environment, _config.IsRebuild, _config.IsReinstall), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await KillServerAsync();
                throw;
            }
        }


        /// <summary>
        /// Stop client
        /// </summary>
        public async Task StopAsync()
        {
            _isCanceled = true;
            await SendPipelineRequestAsync(new PipelineRequest(RequestType.Stop));
            await StopServerAsync(_serverProcess);
        }


        /// <summary>
        /// Load the PythonPipeline
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task LoadAsync(PipelineConfig pipeline, CancellationToken cancellationToken = default)
        {
            try
            {
                _isCanceled = false;
                await StartAsync(cancellationToken);
                await SendPipelineRequestAsync(new PipelineRequest(pipeline), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await KillServerAsync();
                throw;
            }
        }


        /// <summary>
        /// Reload as an asynchronous operation.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task ReloadAsync(PipelineReloadOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                _isCanceled = false;
                await SendPipelineRequestAsync(new PipelineRequest(options), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await KillServerAsync();
                throw;
            }
        }


        /// <summary>
        /// Unload the PythonPipeline
        /// </summary>
        public async Task UnloadAsync()
        {
            _isCanceled = true;
            await SendPipelineRequestAsync(new PipelineRequest(RequestType.PipelineUnload));
            await StopAsync();
        }


        /// <summary>
        /// Run the PythonPipeline
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Tensor<float>> RunAsync(PipelineOptions options)
        {
            _isCanceled = false;
            var response = await SendPipelineRequestAsync(new PipelineRequest(options));
            return response.Tensors.FirstOrDefault();
        }


        /// <summary>
        /// Cancel the PythonPipeline
        /// </summary>
        public async Task CancelAsync()
        {
            _isCanceled = true;
            await SendObjectRequestAsync(new CommandRequest());
        }


        /// <summary>
        /// Kill server.
        /// </summary>
        public async Task KillServerAsync()
        {
            if (_serverProcess is not null)
            {
                _serverProcess.Kill(true);
                await _serverProcess.WaitForExitAsync();
            }
        }


        /// <summary>
        /// Send a Pipeline request to the Server
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<PipelineResponse> SendPipelineRequestAsync(PipelineRequest request, CancellationToken cancellationToken = default)
        {
            await _pipelineChannel.SendMessage(request, cancellationToken);
            var response = await _pipelineChannel.ReceiveMessage<PipelineResponse>(cancellationToken);
            if (response.IsError)
            {
                if (response.IsCanceled)
                    throw new OperationCanceledException(response.Error);

                throw new Exception(response.Error);
            }
            return response;
        }


        /// <summary>
        /// Send a Object request to the Server
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<CommandResponse> SendObjectRequestAsync(CommandRequest request, CancellationToken cancellationToken = default)
        {
            await _commandChannel.SendObject(request, cancellationToken);
            var response = await _commandChannel.ReceiveObject<CommandResponse>(cancellationToken);
            if (response.IsError)
            {
                if (response.IsCanceled)
                    throw new OperationCanceledException(response.Error);

                throw new Exception(response.Error);
            }
            return response;
        }


        /// <summary>
        /// Start server
        /// </summary>
        private async Task StartServerAsync()
        {
            var existingProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == ServerConfig.Name);
            if (existingProcess is not null)
                await StopServerAsync(existingProcess);

            var processInfo = new ProcessStartInfo
            {
                CreateNoWindow = !_config.IsDebugMode,
                UseShellExecute = false,
                FileName = Path.Combine(_config.ServerPath, ServerConfig.Executable),
            };

            // Environment Variables
            if (!_config.Environment.Variables.IsNullOrEmpty())
            {
                foreach (var variable in _config.Environment.Variables)
                    processInfo.Environment.Add(variable);
            }
            _serverProcess = Process.Start(processInfo);
            _processHandler.AddProcess(_serverProcess);
        }


        /// <summary>
        /// Stop server
        /// </summary>
        /// <param name="serverProcess">The server process.</param>
        /// <param name="timeout">The timeout.</param>
        private static async Task StopServerAsync(Process serverProcess, int timeout = 5000)
        {
            using (serverProcess)
            {
                var timeoutDelay = Task.Delay(timeout);
                await Task.WhenAny(timeoutDelay, serverProcess.WaitForExitAsync());
                if (!serverProcess.HasExited)
                {
                    serverProcess.Kill(true);
                    await serverProcess.WaitForExitAsync();
                }
            }
        }


        /// <summary>
        /// Process the progress queue
        /// </summary>
        /// <param name="progressCallback">The progress callback.</param>
        private async Task ProcessProgressQueueAsync(IProgress<PipelineProgress> progressCallback)
        {
            while (!_progressCancellation.IsCancellationRequested)
            {
                try
                {
                    if (!_progressChannel.IsConnected)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    var progress = await _progressChannel.ReceiveObject<PipelineProgress>(_progressCancellation.Token);
                    if (progress == null || _isCanceled)
                        continue;

                    progressCallback?.Report(progress);
                }
                catch (OperationCanceledException) { }
                catch (Exception)
                {
                   // _logger?.LogError(ex, $"[PipelineClient] [ProcessProgressQueueAsync] - An exception occurred processing progress");
                }
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _progressCancellation?.SafeCancel();
            _progressCancellation?.Dispose();
            _progressChannel?.Dispose();
            _commandChannel?.Dispose();
            _pipelineChannel?.Dispose();
            _serverProcess?.Dispose();
        }
    }
}
