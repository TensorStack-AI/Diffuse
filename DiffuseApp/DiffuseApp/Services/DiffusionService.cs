using Diffuse.Common;
using DiffuseApp.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Tensor;
using TensorStack.Python.Common;
using TensorStack.Python.Config;
using TensorStack.Video;

namespace Diffuse.Services
{
    public class DiffusionService : ServiceBase, IDiffusionService
    {
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly IMediaService _mediaService;
        private readonly IEnvironmentService _environmentService;
        private PipelineModel _currentPipeline;
        private PipelineClient _pipelineClient;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isLoaded;
        private bool _isLoading;
        private bool _isExecuting;
        private bool _isCanceling;
        private DiffusionDefaultOptions _defaultOptions;
        private IProgress<PipelineProgress> _progressCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffusionService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DiffusionService(Settings settings, IEnvironmentService environmentService, IMediaService mediaService, ILogger<DiffusionService> logger)
        {
            _logger = logger;
            _settings = settings;
            _mediaService = mediaService;
            _environmentService = environmentService;
        }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        public PipelineModel Pipeline => _currentPipeline;

        /// <summary>
        /// Gets the default options.
        /// </summary>
        public DiffusionDefaultOptions DefaultOptions => _defaultOptions;

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set { SetProperty(ref _isLoaded, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            private set { SetProperty(ref _isLoading, value); NotifyPropertyChanged(nameof(CanCancel)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is executing.
        /// </summary>
        public bool IsExecuting
        {
            get { return _isExecuting; }
            private set { SetProperty(ref _isExecuting, value); NotifyPropertyChanged(nameof(CanCancel)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is canceling.
        /// </summary>
        public bool IsCanceling
        {
            get { return _isCanceling; }
            private set { SetProperty(ref _isCanceling, value); NotifyPropertyChanged(nameof(CanCancel)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can cancel.
        /// </summary>
        public bool CanCancel => _isLoading || _isExecuting;


        /// <summary>
        /// Load the pipeline
        /// </summary>
        /// <param name="config">The configuration.</param>
        public async Task LoadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback)
        {
            IsLoaded = false;
            IsLoading = true;
            IsCanceling = false;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    await UnloadPythonPipeline();

                    _currentPipeline = pipeline;
                    _progressCallback = progressCallback;
                    var device = _currentPipeline.Device;
                    var model = _currentPipeline.DiffusionModel;
                    var controlNet = _currentPipeline.ControlNetModel;
                    _defaultOptions = model.DefaultOptions;

                    var pipelineConfig = new PipelineConfig
                    {
                        Variant = model.Variant,
                        BaseModelPath = model.Path,
                        Pipeline = model.Pipeline,
                        ProcessType = _currentPipeline.ProcessType,
                        Device = device.Type == DeviceType.GPU ? "cuda" : "cpu",
                        DeviceId = device.DeviceId,
                        DeviceBusId = device.PCIBusId,
                        DataType = model.BaseType,
                        QuantDataType = _currentPipeline.DataType,
                        IsOptimizeDeviceEnabled = _settings.IsOptimizeDeviceEnabled,
                        IsOptimizeChannelsEnabled = _settings.IsOptimizeChannelsEnabled,
                        CacheDirectory = Path.GetFullPath(_settings.DirectoryCache),
                        SecureToken = _settings.SecureToken,
                        LoraAdapters = GetLoraAdapters(_currentPipeline.LoraAdapterModel),
                        ControlNet = GetControlNet(controlNet),
                        MemoryMode = SetMemoryMode(_currentPipeline),
                        CheckpointConfig = model.Checkpoint.ToConfig()
                    };

                    var relayedProgressCallback = new Progress<PipelineProgress>(progress => _progressCallback?.Report(progress));
                    _pipelineClient = await _environmentService.CreateClientAsync(_currentPipeline, pipelineConfig, EnvironmentMode.Create, relayedProgressCallback, _cancellationTokenSource.Token);
                    _settings.ScanModels();
                }
                IsLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _pipelineClient?.Dispose();
                _pipelineClient = null;
                _defaultOptions = null;
                _currentPipeline = null;
                throw;
            }
            finally
            {
                IsLoading = false;
                IsCanceling = false;
                _cancellationTokenSource = null;
            }
        }


        /// <summary>
        /// Reload the pipeline
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        public async Task ReloadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback)
        {
            IsLoaded = false;
            IsLoading = true;
            IsCanceling = false;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    _currentPipeline = pipeline;
                    _progressCallback = progressCallback;
                    var reloadOptions = new PipelineReloadOptions
                    {
                        ControlNet = GetControlNet(pipeline.ControlNetModel),
                        LoraAdapters = GetLoraAdapters(pipeline.LoraAdapterModel),
                        ProcessType = pipeline.ProcessType,
                    };

                    await _pipelineClient.ReloadAsync(reloadOptions, _cancellationTokenSource.Token);
                    _settings.ScanModels();
                }
                IsLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _pipelineClient?.Dispose();
                _pipelineClient = null;
                _defaultOptions = null;
                _currentPipeline = null;
                throw;
            }
            finally
            {
                IsLoading = false;
                IsCanceling = false;
                _cancellationTokenSource = null;
            }
        }


        /// <summary>
        /// Execute the upscaler
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task<ImageTensor> GenerateImageAsync(DiffusionInputOptions options)
        {
            IsExecuting = true;
            IsCanceling = false;
            try
            {
                var imageFileName = _mediaService.GetTempFile(MediaType.Image);
                options.Seed = options.Seed > 0 ? options.Seed : Random.Shared.Next();
                options.NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt;
                var generateOptions = new PipelineOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Steps = options.Steps,
                    Steps2 = options.Steps2,
                    GuidanceScale = options.GuidanceScale,
                    GuidanceScale2 = options.GuidanceScale2,
                    Seed = options.Seed,
                    Prompt = options.Prompt,
                    NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt,
                    Scheduler = options.Scheduler,
                    Strength = options.Strength,
                    ControlNetScale = options.ControlNetStrength,
                    InputImages = options.InputImages,
                    InputControlImages = options.InputControlImages,
                    SchedulerOptions = GetSchedulerOptions(options.SchedulerOptions),
                    LoraOptions = GetLoraOptions(options),
                    TempFileName = imageFileName,
                    NoiseCondition = options.NoiseCondition
                };

                var tensorResult = await _pipelineClient.RunAsync(generateOptions);
                return tensorResult.AsImageTensor();
            }
            catch (IOException ex)
            {
                HandleServerError(ex);
                throw new Exception("Pipeline Closed Unexpectedly");
            }
            finally
            {
                IsExecuting = false;
                IsCanceling = false;
            }
        }


        public async Task<VideoInputStream> GenerateVideoAsync(DiffusionInputOptions options)
        {
            IsExecuting = true;
            IsCanceling = false;
            try
            {
                var videoFileName = _mediaService.GetTempFile(MediaType.Video);
                options.Seed = options.Seed > 0 ? options.Seed : Random.Shared.Next();
                options.NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt;
                var generateOptions = new PipelineOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Steps = options.Steps,
                    Steps2 = options.Steps2,
                    GuidanceScale = options.GuidanceScale,
                    GuidanceScale2 = options.GuidanceScale2,
                    Frames = options.Frames,
                    FrameRate = options.FrameRate,
                    Seed = options.Seed,
                    Prompt = options.Prompt,
                    NegativePrompt = options.NegativePrompt,
                    Scheduler = options.Scheduler,
                    Strength = options.Strength,
                    ControlNetScale = options.ControlNetStrength,
                    InputImages = options.InputImages,
                    InputControlImages = options.InputControlImages,
                    SchedulerOptions = GetSchedulerOptions(options.SchedulerOptions),
                    LoraOptions = GetLoraOptions(options),
                    TempFileName = videoFileName,
                    NoiseCondition = options.NoiseCondition,
                    FrameChunk = options.FrameChunk,
                    FrameChunkOverlap = options.FrameChunkOverlap,
                };

                var tensorResult = await _pipelineClient.RunAsync(generateOptions);
                if (tensorResult is null)
                {
                    if (!File.Exists(videoFileName))
                        throw new Exception("Generated video result not found.");

                    return new VideoInputStream(videoFileName);
                }

                var videoTensor = tensorResult.AsVideoTensor(generateOptions.FrameRate);
                await videoTensor.SaveAync(videoFileName);
                return new VideoInputStream(videoFileName);
            }
            catch (IOException ex)
            {
                HandleServerError(ex);
                throw new Exception("Pipeline Closed Unexpectedly");
            }
            finally
            {
                IsExecuting = false;
                IsCanceling = false;
            }
        }


        /// <summary>
        /// Cancel the running task (Load or Execute)
        /// </summary>
        public async Task CancelAsync()
        {
            try
            {
                IsCanceling = true;
                if (_pipelineClient is not null)
                    await _pipelineClient.CancelAsync();
            }
            catch (Exception) { }
            finally
            {
                await _cancellationTokenSource.SafeCancelAsync();
            }
        }


        /// <summary>
        /// Stop/Kill server
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                await _pipelineClient.KillServerAsync();
            }
            catch (Exception) { }
            finally
            {
                IsLoaded = false;
                IsLoading = false;
                IsExecuting = false;
                IsCanceling = false;
                _pipelineClient = null;
            }
        }


        /// <summary>
        /// Unload the pipeline
        /// </summary>
        public async Task UnloadAsync()
        {
            await CancelAsync();
            await UnloadPythonPipeline();
            _currentPipeline = null;
            _defaultOptions = null;
            IsLoaded = false;
            IsLoading = false;
            IsExecuting = false;
            IsCanceling = false;
        }


        private void HandleServerError(Exception exception)
        {
            try
            {
                _pipelineClient?.Dispose();
            }
            catch (Exception) { }
            finally
            {
                _pipelineClient = null;
                _currentPipeline = null;
                _defaultOptions = null;
                IsLoaded = false;
            }
        }


        private static SchedulerOptions GetSchedulerOptions(SchedulerInputOptions schedulerOptions)
        {
            return new SchedulerOptions
            {
                AlgorithmType = schedulerOptions.AlgorithmType,
                BaseShift = schedulerOptions.BaseShift,
                BetaEnd = schedulerOptions.BetaEnd,
                BetaSchedule = schedulerOptions.BetaSchedule,
                BetaStart = schedulerOptions.BetaStart,
                ClipSample = schedulerOptions.ClipSample,
                ClipSampleRange = schedulerOptions.ClipSampleRange,
                DynamicThresholdingRatio = schedulerOptions.DynamicThresholdingRatio,
                Eta = schedulerOptions.Eta,
                LowerOrderFinal = schedulerOptions.LowerOrderFinal,
                MaxShift = schedulerOptions.MaxShift,
                NumTrainTimesteps = schedulerOptions.NumTrainTimesteps,
                PredictionType = schedulerOptions.PredictionType,
                Rho = schedulerOptions.Rho,
                SampleMaxValue = schedulerOptions.SampleMaxValue,
                SChurn = schedulerOptions.SChurn,
                Shift = schedulerOptions.Shift,
                SigmaMax = schedulerOptions.SigmaMax > 0 ? schedulerOptions.SigmaMax : null,
                SigmaMin = schedulerOptions.SigmaMin > 0 ? schedulerOptions.SigmaMin : null,
                SNoise = schedulerOptions.SNoise,
                SolverOrder = schedulerOptions.SolverOrder,
                SolverType = schedulerOptions.SolverType,
                StepsOffset = schedulerOptions.StepsOffset,
                STmax = schedulerOptions.STmax,
                STmin = schedulerOptions.STmin,
                StochasticSampling = schedulerOptions.StochasticSampling,
                Thresholding = schedulerOptions.Thresholding,
                TimestepSpacing = schedulerOptions.TimestepSpacing,
                UseDynamicShifting = schedulerOptions.UseDynamicShifting,
                UseKarrasSigmas = schedulerOptions.UseKarrasSigmas,
                VarianceType = schedulerOptions.VarianceType,
                BaseImageSeqLen = schedulerOptions.BaseImageSeqLen,
                MaxImageSeqLen = schedulerOptions.MaxImageSeqLen
            };
        }



        private static List<LoraConfig> GetLoraAdapters(LoraAdapterModel[] loraAdapterModel)
        {
            if (loraAdapterModel.IsNullOrEmpty())
                return default;

            return [.. loraAdapterModel.Select(x => new LoraConfig
            {
                Path = x.Path,
                Weights = x.Weights,
                Name = x.Key
            })];
        }


        private static List<LoraOptions> GetLoraOptions(DiffusionInputOptions options)
        {
            if (options.LoraOptions.IsNullOrEmpty())
                return default;

            return [.. options.LoraOptions.Select(x => new LoraOptions
            {
                Name = x.Key,
                Strength = x.Strength
            })];
        }


        private static ControlNetConfig GetControlNet(ControlNetModel controlNetModel)
        {
            if (controlNetModel is null)
                return null;

            return new ControlNetConfig
            {
                Name = controlNetModel.Name,
                Path = controlNetModel.Path
            };
        }


        private static MemoryModeType SetMemoryMode(PipelineModel pipeline)
        {
            var memoryMode = pipeline.MemoryMode;
            if (memoryMode == MemoryMode.Auto)
            {
                var memoryProfile = pipeline.DiffusionModel.MemoryProfile.FirstOrDefault(x => x.DataType == pipeline.DataType);
                if (memoryProfile != null)
                {
                    var deviceMemory = pipeline.Device.MemoryGB;
                    var modeIndex = memoryProfile.GetIndex(deviceMemory);
                    memoryMode = Enum.GetValues<MemoryMode>()[modeIndex + 2];
                }
            }

            return memoryMode switch
            {
                MemoryMode.Balanced => MemoryModeType.MultiDevice,
                MemoryMode.Lowest => MemoryModeType.OffloadCPU,
                MemoryMode.Low => MemoryModeType.LowMemOffloadModel,
                MemoryMode.Medium => MemoryModeType.OffloadModel,
                MemoryMode.High => MemoryModeType.LowMemDevice,
                MemoryMode.Highest => MemoryModeType.Device,
                _ => MemoryModeType.OffloadCPU,
            };
        }


        private async Task UnloadPythonPipeline()
        {
            try
            {
                if (_pipelineClient != null)
                    await _pipelineClient.UnloadAsync();
            }
            catch (Exception)
            {
            }
            finally
            {
                _pipelineClient?.Dispose();
                _pipelineClient = null;
            }
        }
    }


    public interface IDiffusionService
    {
        PipelineModel Pipeline { get; }
        DiffusionDefaultOptions DefaultOptions { get; }
        bool IsLoaded { get; }
        bool IsLoading { get; }
        bool IsExecuting { get; }
        bool IsCanceling { get; }
        bool CanCancel { get; }
        Task LoadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback);
        Task ReloadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback);
        Task UnloadAsync();
        Task CancelAsync();
        Task StopAsync();
        Task<ImageTensor> GenerateImageAsync(DiffusionInputOptions options);
        Task<VideoInputStream> GenerateVideoAsync(DiffusionInputOptions options);
    }
}
