using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for VideoUpscaleView.xaml
    /// </summary>
    public partial class VideoUpscaleView : ViewBase
    {
        private readonly ILogger _logger;
        private VideoInputStream _sourceVideo;
        private VideoInputStream _resultVideo;
        private VideoInputStream _compareVideo;
        private UpscaleInputOptions _options;

        public VideoUpscaleView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, IUpscaleService upscaleService, ILogger<VideoUpscaleView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            UpscaleService = upscaleService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }


        public override int Id => (int)View.VideoUpscale;
        public IUpscaleService UpscaleService { get; }
        public AsyncRelayCommand LoadCommand { get; set; }
        public AsyncRelayCommand UnloadCommand { get; set; }
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

        public UpscaleInputOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            if (UpscaleService.IsLoaded)
            {
                //  SelectedModel = UpscaleService.Model;
            }
            return base.OpenAsync(args);
        }


        protected override async Task LoadPipelineAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();
            try
            {
                IsPipelineLoaded = false;
                Progress.Indeterminate("Loading Pipeline...");
                _logger?.LogInformation($"[VideoUpscaleView] [LoadPipelineAsync] - Loading pipeline...");

                await base.LoadPipelineAsync();
                await UpscaleService.UnloadAsync();

                if (CurrentPipeline.UpscaleModel is not null)
                    await UpscaleService.LoadAsync(CurrentPipeline);

                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[VideoUpscaleView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[VideoUpscaleView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoUpscaleView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[VideoUpscaleView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[VideoUpscaleView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await UpscaleService.UnloadAsync();
                _logger?.LogInformation($"[VideoUpscaleView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoUpscaleView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
                await DialogService.ShowErrorAsync("UnloadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            IsPipelineLoaded = false;
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
                _logger?.LogInformation($"[VideoUpscaleView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Upscaler
                var resultVideo = await UpscaleService.ExecuteAsync(new UpscaleVideoRequest
                {
                    VideoStream = _sourceVideo,
                    Options = _options
                }, ProgressCallback);

                Statistics.Stop();

                // Set Result
                ResultVideo = await HistoryService.AddAsync(resultVideo, new UpscaleHistory
                {
                    Options = _options,
                    Model = CurrentPipeline.UpscaleModel.Name,
                    Source = View.VideoUpscale,
                    OriginalWidth = _sourceVideo.Width,
                    OriginalHeight = _sourceVideo.Height,
                    ScaleFactor = CurrentPipeline.UpscaleModel.ScaleFactor
                });
                CompareVideo = _sourceVideo;

                _logger?.LogInformation($"[VideoUpscaleView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[VideoUpscaleView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[VideoUpscaleView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[VideoUpscaleView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return _sourceVideo is not null && UpscaleService.IsLoaded && !UpscaleService.IsExecuting;
        }


        private async Task CancelAsync()
        {
            if (UpscaleService.IsLoading)
                CurrentPipeline = null;

            await UpscaleService.CancelAsync();
        }


        private bool CanCancel()
        {
            return UpscaleService.CanCancel;
        }


        protected async void SelectedUpscalerChanged(object sender, PipelineModel pipeline)
        {
            if (pipeline.UpscaleModel is not null && !pipeline.UpscaleModel.IsValid)
            {
                if (!await pipeline.UpscaleModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Upscale")))
                    CurrentPipeline = default;
            }

            if (CurrentPipeline is not null)
                await LoadPipelineAsync();
        }
    }
}