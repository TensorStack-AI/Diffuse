using DiffuseApp.Common.Config;
using DiffuseApp.Common.Message;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Tensor;
using TensorStack.Python;
using TensorStack.Python.Common;

namespace DiffuseApp.Common
{
    public sealed class PipelineServer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly NamedPipeServerStream _objectPipe;
        private readonly NamedPipeServerStream _messagePipe;
        private readonly Channel<PipelineProgress> _progressQueue;
        private readonly IProgress<PipelineProgress> _progressCallback;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private RequestType _currentState;


        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineServer"/> class.
        /// </summary>
        /// <param name="serverConfig">The server configuration.</param>
        /// <param name="pipelineConfig">The pipeline configuration.</param>
        /// <param name="logger">The logger.</param>
        public PipelineServer(ILogger logger = default)
        {
            _logger = logger;
            _currentState = RequestType.Stop;
            _cancellationTokenSource = new CancellationTokenSource();
            _objectPipe = new NamedPipeServerStream(ServerConfig.ObjectPipeName, PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, ServerConfig.ChunkSize, ServerConfig.ChunkSize);
            _messagePipe = new NamedPipeServerStream(ServerConfig.MessagePipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, ServerConfig.ChunkSize, ServerConfig.ChunkSize);
            _progressQueue = Channel.CreateUnbounded<PipelineProgress>();
            _progressCallback = new Progress<PipelineProgress>(p => _progressQueue.Writer.TryWrite(p));
        }


        /// <summary>
        /// Start the Server loop
        /// </summary>
        /// <param name="isRebuild">if set to <c>true</c> [is rebuild].</param>
        /// <param name="isReinstall">if set to <c>true</c> [is reinstall].</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            CallbackMessage("Starting Server...", "Initialize");
            await WaitForConnectionAsync(cancellationToken);

            // Generate Loop
            CallbackMessage("Initializing...", "Initialize");
            _logger?.LogInformation($"[PythonServer] [StartAsync] Start generate loop.");
            var pythonProxy = default(PythonPipeline);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Read Request
                    var message = await _messagePipe.ReceiveMessage<PipelineRequest>();
                    _logger?.LogInformation($"[PythonServer] [StartAsync] {message.Type} message received.");

                    if (message.Type == RequestType.Stop)
                    {
                        _cancellationTokenSource?.SafeCancel();
                        await _messagePipe.SendResponse(cancellationToken);
                        _logger?.LogInformation($"[PythonServer] [StartAsync] Server stopped.");
                        return;
                    }
                    else if (message.Type == RequestType.Start && _currentState == RequestType.Stop)
                    {
                        _currentState = message.Type;
                        await _messagePipe.SendResponse(cancellationToken);
                        _logger?.LogInformation($"[PythonServer] [StartAsync] Server started.");
                        continue;
                    }
                    else if (message.Type == RequestType.Environment && _currentState == RequestType.Start)
                    {
                        _currentState = message.Type;
                        CallbackMessage("Create Envrironment...", "Initialize");

                        var environment = message.Environment;
                        var pythonService = new PythonManager(environment.Config, _logger);
                        if (pythonService.Exists() && !environment.IsRebuild)
                            await pythonService.LoadAsync(_progressCallback);
                        else
                            await pythonService.CreateAsync(environment.IsRebuild, environment.IsReinstall, _progressCallback);

                        await _messagePipe.SendResponse(cancellationToken);
                        CallbackMessage(string.Empty, "Initialize");
                        continue;
                    }

                    // Environment Loaded
                    if (_currentState == RequestType.Environment)
                    {

                        // Model Load
                        if (message.Type == RequestType.PipelineLoad)
                        {
                            CallbackMessage("Loading Pipeline...", "Initialize");

                            pythonProxy = new PythonPipeline(message.PipelineConfig, _progressCallback, _logger);
                            await pythonProxy.LoadAsync();
                            await _messagePipe.SendResponse(cancellationToken);

                            CallbackMessage(string.Empty, "Initialize");
                            continue;
                        }
                        else if (message.Type == RequestType.PipelineUnload)
                        {
                            CallbackMessage("Loading Pipeline...", "Initialize");

                            await pythonProxy.UnloadAsync();
                            await _messagePipe.SendResponse(cancellationToken);

                            CallbackMessage(string.Empty, "Initialize");
                            continue;
                        }
                        else if (message.Type == RequestType.PipelineRun)
                        {
                            // Generate Response
                            CallbackMessage("Run Pipeline...");

                            var response = default(Tensor<float>);

                            try
                            {
                                if (message.ImageTensorCount > 0)
                                {
                                    message.PipelineOptions.InputImages = message.Tensors
                                        .Take(message.ImageTensorCount)
                                        .Select(x => x.AsImageTensor())
                                        .ToList();
                                }

                                if (message.ControlNetTensorCount > 0)
                                {
                                    message.PipelineOptions.InputControlImages = message.Tensors
                                        .Skip(message.ImageTensorCount)
                                        .Take(message.ControlNetTensorCount)
                                        .Select(x => x.AsImageTensor())
                                        .ToList();
                                }

                                response = await pythonProxy.GenerateAsync(message.PipelineOptions, cancellationToken);

                                _logger?.LogInformation($"[PythonServer] [StartAsync] Response generated.");

                                await _messagePipe.SendMessage(new PipelineResponse
                                {
                                    Tensors = [response]
                                }, cancellationToken: cancellationToken);

                            }
                            catch (Exception ex)
                            {
                                await _messagePipe.SendMessage(new PipelineResponse
                                {
                                    Error = ex.Message
                                }, cancellationToken: cancellationToken);
                            }

                            CallbackMessage(string.Empty);
                        }
                    }

                    _logger?.LogInformation($"[PythonServer] [StartAsync] Response sent.");
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[PythonServer] [StartAsync] An exception occurred");
                    throw;
                }
            }

            pythonProxy?.Dispose();
            _logger?.LogInformation($"[PythonServer] [StartAsync] Generate loop stopped.");
        }


        /// <summary>
        /// Wait for connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation($"[PythonServer][StartAsync] Waiting for connection");
            await _objectPipe.WaitForConnectionAsync(cancellationToken);
            await _messagePipe.WaitForConnectionAsync(cancellationToken);

            // Progress Loop
            _ = ProcessProgressQueueAsync();
            _logger?.LogInformation($"[PythonServer] [StartAsync] Client connected.");
        }


        /// <summary>
        /// Process the progress queue
        /// </summary>
        /// <param name="progressQueue">The progress queue.</param>
        private async Task ProcessProgressQueueAsync()
        {
            await foreach (var progress in _progressQueue.Reader.ReadAllAsync(_cancellationTokenSource.Token))
            {
                try
                {
                    await _objectPipe.SendObject(progress, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"[PythonServer] [ProcessProgressQueueAsync] - An exception occurred processing progress");
                }
            }
        }


        /// <summary>
        /// Send a callback message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void CallbackMessage(string message, string process = "Generate")
        {
            _progressCallback?.Report(new PipelineProgress
            {
                Message = message,
                Process = process
            });
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.SafeCancel();
            _objectPipe?.Dispose();
            _messagePipe?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

    }
}
