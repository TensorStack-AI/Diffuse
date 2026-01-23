using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for TextToVideoView.xaml
    /// </summary>
    public partial class TextToVideoView : ViewBase
    {
        private readonly ILogger _logger;
        private VideoInputStream _resultVideo;
        private VideoInputStream _compareVideo;
        private DiffusionInputOptions _options;
        private UpscaleInputOptions _upscaleOptions;

        public TextToVideoView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<TextToVideoView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            DiffusionService = diffusionService;
            UpscaleService = upscaleService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.TextToVideo;
        public IDiffusionService DiffusionService { get; }
        public IUpscaleService UpscaleService { get; }
        public AsyncRelayCommand ExecuteCommand { get; set; }
        public AsyncRelayCommand CancelCommand { get; set; }

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

        public DiffusionInputOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }

        public UpscaleInputOptions UpscaleOptions
        {
            get { return _upscaleOptions; }
            set { SetProperty(ref _upscaleOptions, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            if (CurrentPipeline is not null && CurrentPipeline != DiffusionService.Pipeline)
            {
                CurrentPipeline = null;
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
                _logger?.LogInformation($"[TextToVideoView] [LoadPipelineAsync] - Loading pipeline..");
                await base.LoadPipelineAsync();

                //DiffusionModel
                if (CurrentPipeline.DiffusionModel is not null)
                {
                    if (!DiffusionService.IsLoaded || CurrentPipeline.IsReloadRequired(DiffusionService.Pipeline))
                    {
                        await DiffusionService.LoadAsync(CurrentPipeline, PythonProgressCallback);
                    }
                }
                else
                {
                    await DiffusionService.UnloadAsync();
                }

                //UpscaleService
                if (CurrentPipeline.UpscaleModel is not null)
                {
                    if (!UpscaleService.IsLoaded || UpscaleService.Pipeline.UpscaleModel != CurrentPipeline.UpscaleModel)
                    {
                        await UpscaleService.LoadAsync(CurrentPipeline);
                    }
                }
                else
                {
                    await UpscaleService.UnloadAsync();
                }

                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[TextToVideoView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[TextToVideoView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[TextToVideoView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[TextToVideoView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[TextToVideoView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                if (DiffusionService.IsLoaded)
                    await DiffusionService.UnloadAsync();
                if (UpscaleService.IsLoaded)
                    await UpscaleService.UnloadAsync();
                _logger?.LogInformation($"[TextToVideoView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[TextToVideoView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                var previousVideo = _resultVideo;
                Progress.Clear();
                Statistics.Clear();
                ResultVideo = default;
                CompareVideo = default;
                await ResultControl.ClearAsync();
                _logger?.LogInformation($"[TextToVideoView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Diffusion
                var options = _options with { Strength = 1 };
                var resultVideo = await DiffusionService.GenerateVideoAsync(options);

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    Progress.Indeterminate("Upscaling Video...");
                    resultVideo = await UpscaleService.ExecuteAsync(new UpscaleVideoRequest
                    {
                        VideoStream = resultVideo,
                        Options = _upscaleOptions
                    }, ProgressCallback);
                }

                Statistics.Stop();

                // Set Result
                ResultVideo = await HistoryService.AddAsync(resultVideo, new DiffusionHistory
                {
                    Options = options,
                    Model = CurrentPipeline.DiffusionModel.Name,
                    LoraModel = CurrentPipeline.LoraAdapterModel?.Name,
                    UpscaleModel = CurrentPipeline.UpscaleModel?.Name,
                    UpscaleOptions = CurrentPipeline.UpscaleModel is not null ? _upscaleOptions : null,
                    Source = View.TextToVideo,
                });
                CompareVideo = previousVideo;

                _logger?.LogInformation($"[TextToVideoView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[TextToVideoView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[TextToVideoView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[TextToVideoView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return DiffusionService.IsLoaded && !DiffusionService.IsExecuting;
        }


        private async Task CancelAsync()
        {
            if (DiffusionService.IsLoading)
                CurrentPipeline = null;

            await DiffusionService.CancelAsync();
        }


        private bool CanCancel()
        {
            return DiffusionService.CanCancel;
        }

    }
}