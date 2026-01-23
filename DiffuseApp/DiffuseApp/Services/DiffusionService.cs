using Diffuse.Common;
using DiffuseApp.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private DiffusionDefaultOptions _defaultOptions;

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
        /// Gets a value indicating whether this instance can cancel.
        /// </summary>
        public bool CanCancel => _isLoading || _isExecuting;


        /// <summary>
        /// Load the upscale pipeline
        /// </summary>
        /// <param name="config">The configuration.</param>
        public async Task LoadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback)
        {
            IsLoaded = false;
            IsLoading = true;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    await UnloadPythonPipeline();

                    _currentPipeline = pipeline;
                    var device = _currentPipeline.Device;
                    var model = _currentPipeline.DiffusionModel;
                    var controlNet = _currentPipeline.ControlNetModel;
                    _defaultOptions = model.DefaultOptions;

                    var pipelineConfig = new PipelineConfig
                    {
                        BaseModelPath = model.Path,
                        ControlNetPath = controlNet?.Path,
                        Pipeline = model.Pipeline,
                        ProcessType = _currentPipeline.ProcessType,
                        Device = device.Type == DeviceType.GPU ? "cuda" : "cpu",
                        DeviceId = device.DeviceId,
                        DataType = model.BaseType,
                        QuantDataType = _currentPipeline.DataType,
                        CacheDirectory = Path.GetFullPath(_settings.DirectoryCache),
                        SecureToken = _settings.SecureToken,
                        LoraAdapters = GetLoraAdapters(_currentPipeline.LoraAdapterModel),
                        MemoryMode = SetMemoryMode(_currentPipeline),
                        CheckpointConfig = model.Checkpoint is null ? default : new CheckpointConfig
                        {
                            VaeCheckpoint = model.Checkpoint.VaeCheckpoint,
                            ModelCheckpoint = model.Checkpoint.ModelCheckpoint,
                            TextEncoderCheckpoint = model.Checkpoint.TextEncoderCheckpoint
                        }
                    };

                    _pipelineClient = await _environmentService.CreateClientAsync(_currentPipeline, pipelineConfig, progressCallback, _cancellationTokenSource.Token);
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
            try
            {
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
                    LoraOptions = GetLoraOptions(_currentPipeline.LoraAdapterModel, options),
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
            }
        }


        public async Task<VideoInputStream> GenerateVideoAsync(DiffusionInputOptions options)
        {
            IsExecuting = true;
            try
            {
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
                    Frames = _defaultOptions.Frames,
                    FrameRate = _defaultOptions.FrameRate,
                    Seed = options.Seed,
                    Prompt = options.Prompt,
                    NegativePrompt = options.NegativePrompt,
                    Scheduler = options.Scheduler,
                    Strength = options.Strength,
                    ControlNetScale = options.ControlNetStrength,
                    InputImages = options.InputImages,
                    InputControlImages = options.InputControlImages,
                    SchedulerOptions = GetSchedulerOptions(options.SchedulerOptions),
                    LoraOptions = GetLoraOptions(_currentPipeline.LoraAdapterModel, options),
                };

                var videoFileName = _mediaService.GetTempVideoFile();
                var tensorResult = await _pipelineClient.RunAsync(generateOptions);
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
            }
        }


        /// <summary>
        /// Cancel the running task (Load or Execute)
        /// </summary>
        public async Task CancelAsync()
        {
            try
            {
                if (_pipelineClient is not null)
                    await _pipelineClient.CancelAsync();
            }
            catch (Exception)
            {
            }
            await _cancellationTokenSource.SafeCancelAsync();
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



        private static List<LoraConfig> GetLoraAdapters(LoraAdapterModel loraAdapterModel)
        {
            if (loraAdapterModel is null)
                return default;

            return [new LoraConfig
            {
                Path = loraAdapterModel.Path,
                Weights = loraAdapterModel.Weights,
                Name = loraAdapterModel.Key
            }];
        }


        private static List<LoraOptions> GetLoraOptions(LoraAdapterModel loraAdapterModel, Common.DiffusionInputOptions options)
        {
            if (loraAdapterModel is null)
                return default;

            return [new LoraOptions
            {
                Name = loraAdapterModel.Key,
                Strength = options.LoraStrength
            }];
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
        bool CanCancel { get; }
        Task LoadAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback);
        Task UnloadAsync();
        Task CancelAsync();
        Task<ImageTensor> GenerateImageAsync(DiffusionInputOptions options);
        Task<VideoInputStream> GenerateVideoAsync(DiffusionInputOptions options);
    }
}
