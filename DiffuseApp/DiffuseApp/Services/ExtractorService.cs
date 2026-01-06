using Diffuse.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Common;
using TensorStack.Common.Pipeline;
using TensorStack.Common.Tensor;
using TensorStack.Common.Video;
using TensorStack.Extractors.Common;
using TensorStack.Extractors.Pipelines;
using TensorStack.Providers;
using TensorStack.Video;

namespace Diffuse.Services
{
    public class ExtractorService : ServiceBase, IExtractorService
    {
        private readonly Settings _settings;
        private readonly IMediaService _mediaService;
        private PipelineModel _currentPipeline;
        private IPipeline _extractorPipeline;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isLoaded;
        private bool _isLoading;
        private bool _isExecuting;
        private ExtractorConfig _currentConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractorService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ExtractorService(Settings settings, IMediaService mediaService)
        {
            _settings = settings;
            _mediaService = mediaService;
        }


        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        public PipelineModel Pipeline => _currentPipeline;

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
        /// Load the pipeline
        /// </summary>
        /// <param name="config">The configuration.</param>
        public async Task LoadAsync(PipelineModel pipeline)
        {
            try
            {
                IsLoaded = false;
                IsLoading = true;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var cancellationToken = _cancellationTokenSource.Token;
                    if (_extractorPipeline != null)
                    {
                        if (_currentConfig.Path == pipeline.ExtractorModel.Path)
                            return; // Already loaded

                        await _extractorPipeline.UnloadAsync(cancellationToken);
                    }

                    _currentPipeline = pipeline;
                    var device = _currentPipeline.Device;
                    var model = _currentPipeline.ExtractorModel;
                    _currentConfig = new ExtractorConfig
                    {
                        Path = model.Path,
                        Channels = model.Channels,
                        Normalization = model.Normalization,
                        OutputChannels = model.OutputChannels,
                        OutputNormalization = model.OutputNormalization,
                        IsDynamicOutput = model.IsDynamicOutput,
                        SampleSize = model.SampleSize
                    };

                    _currentConfig.SetProvider(device.GetProvider());
                    _extractorPipeline = model.Type switch
                    {
                        ExtractorType.Pose => PosePipeline.Create(_currentConfig),
                        ExtractorType.Background => BackgroundPipeline.Create(_currentConfig),
                        _ => ExtractorPipeline.Create(_currentConfig)
                    };
                    await Task.Run(() => _extractorPipeline.LoadAsync(cancellationToken), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _extractorPipeline?.Dispose();
                _extractorPipeline = null;
                _currentConfig = null;
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
        /// Execute the image ExtractorPipeline
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task<ImageTensor> ExecuteAsync(ExtractorImageRequest options)
        {
            try
            {
                IsExecuting = true;
                var imageTensor = _currentPipeline.ExtractorModel.Type switch
                {
                    ExtractorType.Default => await ExecuteDefaultAsync(options),
                    ExtractorType.Background => await ExecuteBackgroundAsync(options),
                    ExtractorType.Pose => await ExecutePoseAsync(options),
                    _ => throw new NotImplementedException()
                };

                return imageTensor;
            }
            finally
            {
                IsExecuting = false;
            }
        }


        public async Task<VideoInputStream> ExecuteAsync(ExtractorVideoRequest options, IProgress<RunProgress> progressCallback)
        {
            try
            {
                IsExecuting = true;
                var videoStream = _currentPipeline.ExtractorModel.Type switch
                {
                    ExtractorType.Default => await ExecuteDefaultAsync(options, progressCallback),
                    ExtractorType.Background => await ExecuteBackgroundAsync(options, progressCallback),
                    ExtractorType.Pose => await ExecutePoseAsync(options, progressCallback),
                    _ => throw new NotImplementedException()
                };

                return videoStream;
            }
            finally
            {
                IsExecuting = false;
            }
        }




        /// <summary>
        /// Execute the image ExtractorPipeline
        /// </summary>
        /// <param name="request">The request.</param>
        private async Task<ImageTensor> ExecuteDefaultAsync(ExtractorImageRequest options)
        {
            var pipeline = _extractorPipeline as ExtractorPipeline;
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                return await Task.Run(() => pipeline.RunAsync(new ExtractorImageOptions
                {
                    Image = options.Image,
                    IsInverted = options.IsInverted,
                    MaxTileSize = options.MaxTileSize,
                    TileMode = options.TileMode,
                    TileOverlap = options.TileOverlap,
                    MergeInput = options.MergeInput
                }, cancellationToken: _cancellationTokenSource.Token));
            }
        }


        /// <summary>
        /// Execute the image BackgroundPipeline
        /// </summary>
        /// <param name="request">The request.</param>
        private async Task<ImageTensor> ExecuteBackgroundAsync(ExtractorImageRequest options)
        {
            var pipeline = _extractorPipeline as BackgroundPipeline;
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                return await Task.Run(() => pipeline.RunAsync(new BackgroundImageOptions
                {
                    Mode = options.Mode,
                    Image = options.Image
                }, cancellationToken: _cancellationTokenSource.Token));
            }
        }


        /// <summary>
        /// Execute the image PosePipeline
        /// </summary>
        /// <param name="request">The request.</param>
        private async Task<ImageTensor> ExecutePoseAsync(ExtractorImageRequest options)
        {
            var pipeline = _extractorPipeline as PosePipeline;
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                return await Task.Run(() => pipeline.RunAsync(new PoseImageOptions
                {
                    Image = options.Image,
                    BodyConfidence = options.BodyConfidence,
                    BoneRadius = options.BoneRadius,
                    BoneThickness = options.BoneThickness,
                    ColorAlpha = options.ColorAlpha,
                    Detections = options.Detections,
                    IsTransparent = options.IsTransparent,
                    JointConfidence = options.JointConfidence,
                    JointRadius = options.JointRadius,
                }, cancellationToken: _cancellationTokenSource.Token));
            }
        }


        /// <summary>
        /// Execute the video ExtractorPipeline
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <returns>A Task&lt;VideoInputStream&gt; representing the asynchronous operation.</returns>
        private async Task<VideoInputStream> ExecuteDefaultAsync(ExtractorVideoRequest options, IProgress<RunProgress> progressCallback)
        {
            var pipeline = _extractorPipeline as ExtractorPipeline;
            var resultVideoFile = FileHelper.RandomFileName(_settings.DirectoryTemp, "mp4");
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                var frameCount = options.VideoStream.FrameCount;
                var cancellationToken = _cancellationTokenSource.Token;

                async Task<VideoFrame> FrameProcessor(VideoFrame frame)
                {
                    var processedFrame = await pipeline.RunAsync(new ExtractorImageOptions
                    {
                        Image = frame.Frame,
                        IsInverted = options.IsInverted,
                        MaxTileSize = options.MaxTileSize,
                        TileMode = options.TileMode,
                        TileOverlap = options.TileOverlap,
                        MergeInput = options.MergeInput
                    }, cancellationToken: cancellationToken);

                    progressCallback.Report(new RunProgress(frame.Index, frameCount));
                    return new VideoFrame(frame.Index, processedFrame, frame.SourceFrameRate, frame.AuxFrame);
                }

                return await _mediaService.SaveWithAudioAsync(options.VideoStream, resultVideoFile, FrameProcessor, cancellationToken);
            }
        }


        /// <summary>
        /// Execute the video BackgroundPipeline
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <returns>A Task&lt;VideoInputStream&gt; representing the asynchronous operation.</returns>
        private async Task<VideoInputStream> ExecuteBackgroundAsync(ExtractorVideoRequest options, IProgress<RunProgress> progressCallback)
        {
            var pipeline = _extractorPipeline as BackgroundPipeline;
            var resultVideoFile = FileHelper.RandomFileName(_settings.DirectoryTemp, "mp4");
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                var frameCount = options.VideoStream.FrameCount;
                var cancellationToken = _cancellationTokenSource.Token;

                async Task<VideoFrame> FrameProcessor(VideoFrame frame)
                {
                    var processedFrame = await pipeline.RunAsync(new BackgroundImageOptions
                    {
                        Image = frame.Frame,
                        Mode = options.Mode,
                        IsTransparentSupported = false
                    }, cancellationToken: cancellationToken);

                    progressCallback.Report(new RunProgress(frame.Index, frameCount));
                    return new VideoFrame(frame.Index, processedFrame, frame.SourceFrameRate, frame.AuxFrame);
                }

                return await _mediaService.SaveWithAudioAsync(options.VideoStream, resultVideoFile, FrameProcessor, cancellationToken);
            }
        }


        /// <summary>
        /// Execute the video PosePipeline
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <returns>A Task&lt;VideoInputStream&gt; representing the asynchronous operation.</returns>
        private async Task<VideoInputStream> ExecutePoseAsync(ExtractorVideoRequest options, IProgress<RunProgress> progressCallback)
        {
            var pipeline = _extractorPipeline as PosePipeline;
            var resultVideoFile = FileHelper.RandomFileName(_settings.DirectoryTemp, "mp4");
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                var frameCount = options.VideoStream.FrameCount;
                var cancellationToken = _cancellationTokenSource.Token;

                async Task<VideoFrame> FrameProcessor(VideoFrame frame)
                {
                    var processedFrame = await pipeline.RunAsync(new PoseImageOptions
                    {
                        Image = frame.Frame,
                        BodyConfidence = options.BodyConfidence,
                        BoneRadius = options.BoneRadius,
                        BoneThickness = options.BoneThickness,
                        ColorAlpha = options.ColorAlpha,
                        Detections = options.Detections,
                        IsTransparent = options.IsTransparent,
                        JointConfidence = options.JointConfidence,
                        JointRadius = options.JointRadius,
                    }, cancellationToken: cancellationToken);

                    progressCallback.Report(new RunProgress(frame.Index, frameCount));
                    return new VideoFrame(frame.Index, processedFrame, frame.SourceFrameRate, frame.AuxFrame);
                }

                return await _mediaService.SaveWithAudioAsync(options.VideoStream, resultVideoFile, FrameProcessor, cancellationToken);
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
            if (_extractorPipeline != null)
            {
                await _cancellationTokenSource.SafeCancelAsync();
                await _extractorPipeline.UnloadAsync();
                _extractorPipeline?.Dispose();
                _extractorPipeline = null;
                _currentConfig = null;
                _currentPipeline = null;
            }

            IsLoaded = false;
            IsLoaded = false;
            IsExecuting = false;
        }
    }


    public interface IExtractorService
    {
        PipelineModel Pipeline { get; }
        bool IsLoaded { get; }
        bool IsLoading { get; }
        bool IsExecuting { get; }
        bool CanCancel { get; }
        Task LoadAsync(PipelineModel pipeline);
        Task UnloadAsync();
        Task CancelAsync();
        Task<ImageTensor> ExecuteAsync(ExtractorImageRequest options);
        Task<VideoInputStream> ExecuteAsync(ExtractorVideoRequest options, IProgress<RunProgress> progressCallback);
    }


    public record ExtractorImageRequest
    {
        // Default
        public TileMode TileMode { get; init; }
        public int MaxTileSize { get; init; }
        public int TileOverlap { get; init; }
        public bool IsInverted { get; init; }
        public bool MergeInput { get; init; }
        public bool IsTransparent { get; set; }

        // Background
        public BackgroundMode Mode { get; init; }


        // Pose
        public int Detections { get; set; } = 0;
        public float BodyConfidence { get; init; } = 0.4f;
        public float JointConfidence { get; init; } = 0.1f;
        public float ColorAlpha { get; init; } = 0.8f;
        public float JointRadius { get; init; } = 7f;
        public float BoneRadius { get; init; } = 8f;
        public float BoneThickness { get; init; } = 1f;


        public ImageTensor Image { get; set; }
    }


    public record ExtractorVideoRequest
    {
        // Default
        public TileMode TileMode { get; init; }
        public int MaxTileSize { get; init; }
        public int TileOverlap { get; init; }
        public bool IsInverted { get; init; }
        public bool MergeInput { get; init; }
        public bool IsTransparent { get; set; }

        // Background
        public BackgroundMode Mode { get; init; }


        // Pose
        public int Detections { get; set; } = 0;
        public float BodyConfidence { get; init; } = 0.4f;
        public float JointConfidence { get; init; } = 0.1f;
        public float ColorAlpha { get; init; } = 0.8f;
        public float JointRadius { get; init; } = 7f;
        public float BoneRadius { get; init; } = 8f;
        public float BoneThickness { get; init; } = 1f;
        public VideoInputStream VideoStream { get; init; }
    }



}
