using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TensorStack.Common.Pipeline;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for VideoInterpolateView.xaml
    /// </summary>
    public partial class VideoInterpolateView : ViewBase
    {
        private readonly ILogger _logger;
        private VideoInputStream _sourceVideo;
        private VideoInputStream _resultVideo;
        private VideoInputStream _compareVideo;
        private int _multiplier = 2;

        public VideoInterpolateView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, IInterpolationService interpolationService, ILogger<VideoInterpolateView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            InterpolationService = interpolationService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.VideoInterpolate;
        public IInterpolationService InterpolationService { get; }
        public AsyncRelayCommand ExecuteCommand { get; set; }
        public AsyncRelayCommand CancelCommand { get; set; }

        public VideoInputStream SourceVideo
        {
            get { return _sourceVideo; }
            set { SetProperty(ref _sourceVideo, value); }
        }

        public VideoInputStream ResultVideo
        {
            get { return _resultVideo; }
            set { SetProperty(ref _resultVideo, value); }
        }

        public VideoInputStream CompareVideo
        {
            get { return _compareVideo; }
            set { SetProperty(ref _compareVideo, value); }
        }

        public int Multiplier
        {
            get { return _multiplier; }
            set { SetProperty(ref _multiplier, value); }
        }


        protected override async Task LoadPipelineAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();
            try
            {
                Progress.Indeterminate("Loading Pipeline...");
                _logger?.LogInformation($"[VideoInterpolateView] [LoadPipelineAsync] - Loading pipeline...");

                await base.LoadPipelineAsync();
                await InterpolationService.LoadAsync(CurrentPipeline.Device);

                _logger?.LogInformation($"[VideoInterpolateView] [LoadPipelineAsync] - Loading pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[VideoInterpolateView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoInterpolateView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[VideoInterpolateView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[VideoInterpolateView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await InterpolationService.UnloadAsync();
                _logger?.LogInformation($"[VideoInterpolateView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoInterpolateView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
                await DialogService.ShowErrorAsync("UnloadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
        }


        private async Task ExecuteAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();
            try
            {
                Progress.Clear();
                Statistics.Clear();
                ResultVideo = default;
                CompareVideo = default;
                await ResultControl.ClearAsync();
                _logger?.LogInformation($"[VideoInterpolateView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Interpolation
                var resultVideo = await InterpolationService.ExecuteAsync(new InterpolationRequest
                {
                    VideoStream = _sourceVideo,
                    Frames = _sourceVideo.FrameCount,
                    FrameRate = _sourceVideo.FrameRate,
                    Multiplier = _multiplier
                }, ProgressCallback);

                Statistics.Stop();

                // Set Result
                CompareVideo = _resultVideo;
                ResultVideo = await HistoryService.AddAsync(resultVideo, new InterpolateHistory
                {
                    Multiplier = _multiplier,
                    Source = View.VideoInterpolate,
                    FrameRate = resultVideo.FrameRate,
                    OriginalFrameRate = _sourceVideo.FrameRate
                });

                _logger?.LogInformation($"[VideoInterpolateView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[VideoInterpolateView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[VideoInterpolateView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[VideoInterpolateView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return _sourceVideo is not null && InterpolationService.IsLoaded && !InterpolationService.IsExecuting;
        }


        private async Task CancelAsync()
        {
            if (InterpolationService.IsLoading)
                CurrentPipeline = null;

            await InterpolationService.CancelAsync();
        }


        private bool CanCancel()
        {
            return InterpolationService.CanCancel;
        }


        protected async void SelectedInterpolationChanged(object sender, PipelineModel pipeline)
        {
            await LoadPipelineAsync();
        }
    }
}