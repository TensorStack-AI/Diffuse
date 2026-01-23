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
    /// Interaction logic for VideoExtractView.xaml
    /// </summary>
    public partial class VideoExtractView : ViewBase
    {
        private readonly ILogger _logger;
        private VideoInputStream _sourceVideo;
        private VideoInputStream _resultVideo;
        private VideoInputStream _compareVideo;
        private ExtractInputOptions _options;

        public VideoExtractView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, IExtractService extractService, ILogger<VideoExtractView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            ExtractService = extractService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.VideoExtract;
        public IExtractService ExtractService { get; }
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

        public ExtractInputOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            if (ExtractService.IsLoaded)
            {
                // SelectedModel = ExtractService.Model;
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
                _logger?.LogInformation($"[VideoExtractView] [LoadPipelineAsync] - Loading pipeline...");

                await base.LoadPipelineAsync();
                await ExtractService.UnloadAsync();

                if (CurrentPipeline.ExtractModel is not null)
                    await ExtractService.LoadAsync(CurrentPipeline);

                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[VideoExtractView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[VideoExtractView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoExtractView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[VideoExtractView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[VideoExtractView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await ExtractService.UnloadAsync();
                _logger?.LogInformation($"[VideoExtractView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[VideoExtractView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                _logger?.LogInformation($"[VideoExtractView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Extractor
                var resultVideo = await ExtractService.ExecuteAsync(new ExtractVideoRequest
                {
                    VideoStream = _sourceVideo,
                    Options = _options,
                }, ProgressCallback);

                Statistics.Stop();

                // Set Result
                ResultVideo = await HistoryService.AddAsync(resultVideo, new ExtractHistory
                {
                    Options = _options,
                    Model = CurrentPipeline.ExtractModel.Name,
                    ExtractorType = CurrentPipeline.ExtractModel.Type,
                    Source = View.VideoExtract,
                });
                CompareVideo = _sourceVideo;

                _logger?.LogInformation($"[VideoExtractView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[VideoExtractView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[VideoExtractView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[VideoExtractView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return _sourceVideo is not null && ExtractService.IsLoaded && !ExtractService.IsExecuting;
        }


        private async Task CancelAsync()
        {
            if (ExtractService.IsLoading)
                CurrentPipeline = null;

            await ExtractService.CancelAsync();
        }


        private bool CanCancel()
        {
            return ExtractService.CanCancel;
        }


        protected async void SelectedExtractorChanged(object sender, PipelineModel pipeline)
        {
            if (pipeline.ExtractModel is not null && !pipeline.ExtractModel.IsValid)
            {
                if (!await pipeline.ExtractModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Extract")))
                    CurrentPipeline = default;
            }

            if (CurrentPipeline is not null)
                await LoadPipelineAsync();
        }
    }
}