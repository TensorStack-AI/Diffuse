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
        private readonly NamedPipeClientStream _objectPipe;
        private readonly NamedPipeClientStream _messagePipe;
        private readonly ClientConfig _config;
        private readonly ProcessLifetimeHandler _processhandler;
        private readonly IProgress<PipelineProgress> _progressCallback;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Process _serverProcess;

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
            _processhandler = new ProcessLifetimeHandler();
            _cancellationTokenSource = new CancellationTokenSource();
            _objectPipe = new NamedPipeClientStream(".", ServerConfig.ObjectPipeName, PipeDirection.In, PipeOptions.Asynchronous);
            _messagePipe = new NamedPipeClientStream(".", ServerConfig.MessagePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _ = ProcessProgressQueueAsync(_progressCallback);
        }


        /// <summary>
        /// Start client as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Start Server
            await StartServerAsync();

            // Connect Pipes
            await Task.WhenAll(_objectPipe.ConnectAsync(cancellationToken), _messagePipe.ConnectAsync(cancellationToken));

            // Start Environment
            await SendRequestAsync(new PipelineRequest(RequestType.Start), CancellationToken.None);
            await SendRequestAsync(new PipelineRequest(_config.Environment, _config.IsRebuild, _config.IsReinstall), cancellationToken);
        }


        /// <summary>
        /// Load the PythonPipeline
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task LoadAsync(PipelineConfig pipeline, CancellationToken cancellationToken = default)
        {
            await StartAsync(cancellationToken);
            await SendRequestAsync(new PipelineRequest(pipeline), cancellationToken);
        }


        /// <summary>
        /// Unload the PythonPipeline
        /// </summary>
        public async Task UnloadAsync()
        {
            await SendRequestAsync(new PipelineRequest(RequestType.PipelineUnload), CancellationToken.None);
            await StopClientAsync();
        }


        /// <summary>
        /// Run the PythonPipeline
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Tensor<float>> RunAsync(PipelineOptions options, CancellationToken cancellationToken = default)
        {
            var response = await SendRequestAsync(new PipelineRequest(options), cancellationToken);
            return response.Tensors.FirstOrDefault();
        }


        /// <summary>
        /// Send as request to the Server
        /// </summary>
        /// <param name="request">The request.</param>
        private async Task<PipelineResponse> SendRequestAsync(PipelineRequest request, CancellationToken cancellationToken)
        {
            await _messagePipe.SendMessage(request, cancellationToken);
            var response = await _messagePipe.ReceiveMessage<PipelineResponse>(cancellationToken);
            if (response.IsError)
                throw new Exception(response.Error);

            return response;
        }


        /// <summary>
        /// Stop client
        /// </summary>
        private async Task StopClientAsync()
        {
            await _cancellationTokenSource.SafeCancelAsync();
            await SendRequestAsync(new PipelineRequest(RequestType.Stop), CancellationToken.None);
            await StopServerAsync(_serverProcess);
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
                CreateNoWindow = true,
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
            _processhandler.AddProcess(_serverProcess);
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
        /// <param name="statusPipe">The status pipe.</param>
        /// <param name="progressCallback">The progress callback.</param>
        private async Task ProcessProgressQueueAsync(IProgress<PipelineProgress> progressCallback)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!_objectPipe.IsConnected)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    progressCallback?.Report(await _objectPipe.ReceiveObject<PipelineProgress>(_cancellationTokenSource.Token));
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"[PythonClient] [ProcessProgressQueueAsync] - An exception occurred processing progress");
                }
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.SafeCancel();
            _objectPipe?.Dispose();
            _messagePipe?.Dispose();
            _serverProcess?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
