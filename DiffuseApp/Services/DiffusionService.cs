using CSnakes.Runtime;
using Diffuse.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Tensor;
using TensorStack.Python;
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
        private PythonPipeline _diffusionPipeline;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isLoaded;
        private bool _isLoading;
        private bool _isExecuting;
        private DiffusionDefaultOptions _defaultOptions;
        private IPythonEnvironment _pythonEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffusionService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DiffusionService(Settings settings, IMediaService mediaService, IEnvironmentService environmentService, ILogger<DiffusionService> logger)
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
            try
            {
                IsLoaded = false;
                IsLoading = true;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var cancellationToken = _cancellationTokenSource.Token;
                    await UnloadPythonPipeline();

                    _currentPipeline = pipeline;

                    var device = _currentPipeline.Device;
                    var model = _currentPipeline.DiffusionModel;
                    var controlNet = _currentPipeline.ControlNetModel;

                    var pipelineConfig = new PipelineConfig
                    {
                        Path = model.ModelUrl,
                        ControlNetPath = controlNet?.Path,
                        Pipeline = model.Pipeline,
                        ProcessType = _currentPipeline.ProcessType,
                        Device = "cuda",
                        DeviceId = device.DeviceId,
                        DataType = _currentPipeline.DataType,
                        CacheDirectory = Path.GetFullPath(_settings.DirectoryCache),
                        SecureToken = _settings.SecureToken,
                        LoraAdapters = GetLoraAdapters(_currentPipeline.LoraAdapterModel)
                    };

                    SetMemoryMode(pipeline, pipelineConfig);

                    _defaultOptions = model.DefaultOptions;
                    _diffusionPipeline = await LoadPythonPipelineAsync(pipelineConfig, progressCallback, _logger);
                }
            }
            catch (OperationCanceledException)
            {
                _diffusionPipeline?.Dispose();
                _diffusionPipeline = null;
                _defaultOptions = null;
                _currentPipeline = null;
                throw;
            }
            finally
            {
                IsLoaded = true;
                IsLoading = false;
            }
        }



        /// <summary>
        /// Execute the upscaler
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task<ImageTensor> GenerateImageAsync(Common.GenerateOptions options)
        {
            try
            {
                IsExecuting = true;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    options.Seed = options.Seed > 0 ? options.Seed : Random.Shared.Next();
                    options.NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt;
                    var generateOptions = new PipelineOptions
                    {
                        Width = options.Width,
                        Height = options.Height,
                        Steps = options.Steps,
                        GuidanceScale = options.GuidanceScale,
                        Seed = options.Seed,
                        Prompt = options.Prompt,
                        NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt,
                        Scheduler = options.Scheduler,
                        Strength = options.Strength,
                        ControlNetScale = options.ControlNetStrength,
                        InputImages = options.InputImages,
                        InputControlImages = options.InputControlImages,
                        LoraOptions = GetLoraOptions(_currentPipeline.LoraAdapterModel, options)
                    };

                    var tensorResult = await Task.Run(() => _diffusionPipeline.GenerateAsync(generateOptions, _cancellationTokenSource.Token));
                    return tensorResult.AsImageTensor();
                }
            }
            finally
            {
                IsExecuting = false;
            }
        }


        public async Task<VideoInputStream> GenerateVideoAsync(GenerateOptions options)
        {
            try
            {
                IsExecuting = true;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    options.Seed = options.Seed > 0 ? options.Seed : Random.Shared.Next();
                    options.NegativePrompt = options.GuidanceScale > 1f && string.IsNullOrEmpty(options.NegativePrompt) ? " " : options.NegativePrompt;
                    var generateOptions = new PipelineOptions
                    {
                        Width = options.Width,
                        Height = options.Height,
                        Steps = options.Steps,
                        GuidanceScale = options.GuidanceScale,
                        Frames = _defaultOptions.Frames,
                        FrameRate = _defaultOptions.FrameRate,
                        Seed = options.Seed,
                        Prompt = options.Prompt,
                        NegativePrompt = options.NegativePrompt,
                        Scheduler = options.Scheduler,
                        Strength = options.Strength,
                        ControlNetScale = options.ControlNetStrength,
                        Shift = options.Shift,
                        InputImages = options.InputImages,
                        InputControlImages = options.InputControlImages,
                        LoraOptions = GetLoraOptions(_currentPipeline.LoraAdapterModel, options)
                    };

                    var videoFileName = _mediaService.GetTempVideoFile();
                    var tensorResult = await Task.Run(() => _diffusionPipeline.GenerateAsync(generateOptions, _cancellationTokenSource.Token));

                    var videotensor = tensorResult.AsVideoTensor(generateOptions.FrameRate);
                    await videotensor.SaveAync(videoFileName);
                    return new VideoInputStream(videoFileName);
                }
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
            await _cancellationTokenSource.SafeCancelAsync();
        }


        /// <summary>
        /// Unload the pipeline
        /// </summary>
        public async Task UnloadAsync()
        {
            await _cancellationTokenSource.SafeCancelAsync();
            await UnloadPythonPipeline();

            _currentPipeline = null;
            IsLoaded = false;
            IsLoading = false;
            IsExecuting = false;
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


        private static List<LoraOptions> GetLoraOptions(LoraAdapterModel loraAdapterModel, Common.GenerateOptions options)
        {
            if (loraAdapterModel is null)
                return default;

            return [new LoraOptions
            {
                Name = loraAdapterModel.Key,
                Strength = options.LoraStrength
            }];
        }


        private static void SetMemoryMode(PipelineModel pipeline, PipelineConfig pipelineConfig)
        {
            var memoryMode = pipeline.MemoryMode;
            if (memoryMode == MemoryMode.Auto)
            {
                int[] modes = pipeline.DiffusionModel.MemoryModes;
                if (modes?.Length == 4)
                {
                    var deviceMemory = pipeline.Device.MemoryGB;
                    var modeIndex = Math.Max(0, Array.FindLastIndex(modes, m => m <= deviceMemory));
                    memoryMode = Enum.GetValues<MemoryMode>()[modeIndex + 1];
                }
            }

            if (memoryMode == MemoryMode.Minimum)
            {
                pipelineConfig.IsVaeTilingEnabled = true;
                pipelineConfig.IsVaeSlicingEnabled = true;
                pipelineConfig.IsFullOffloadEnabled = true;
                pipelineConfig.IsModelOffloadEnabled = false;
            }
            else if (memoryMode == MemoryMode.Medium)
            {
                pipelineConfig.IsVaeTilingEnabled = true;
                pipelineConfig.IsVaeSlicingEnabled = true;
                pipelineConfig.IsFullOffloadEnabled = false;
                pipelineConfig.IsModelOffloadEnabled = true;
            }
            else if (memoryMode == MemoryMode.High)
            {
                pipelineConfig.IsVaeTilingEnabled = false;
                pipelineConfig.IsVaeSlicingEnabled = false;
                pipelineConfig.IsFullOffloadEnabled = false;
                pipelineConfig.IsModelOffloadEnabled = true;
            }
            else if (memoryMode == MemoryMode.Maximum)
            {
                pipelineConfig.IsVaeTilingEnabled = false;
                pipelineConfig.IsVaeSlicingEnabled = false;
                pipelineConfig.IsFullOffloadEnabled = false;
                pipelineConfig.IsModelOffloadEnabled = false;
            }
        }


        private async Task<PythonPipeline> LoadPythonPipelineAsync(PipelineConfig pipelineConfig, IProgress<PipelineProgress> progressCallback, ILogger logger)
        {
            return await Task.Run(async () =>
             {
                 var diffusionPipeline = new PythonPipeline(pipelineConfig, progressCallback, logger);
                 await diffusionPipeline.LoadAsync();
                 return diffusionPipeline;
             }, CancellationToken.None);
        }


        private async Task UnloadPythonPipeline()
        {
            if (_diffusionPipeline != null)
            {
                await _diffusionPipeline.UnloadAsync();
                _diffusionPipeline.Dispose();
                _diffusionPipeline = null;
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
        Task<ImageTensor> GenerateImageAsync(GenerateOptions options);
        Task<VideoInputStream> GenerateVideoAsync(GenerateOptions options);
    }
}
