using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TensorStack.Image;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for TextToImageView.xaml
    /// </summary>
    public partial class TextToImageView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private DiffusionInputOptions _options;
        private UpscaleInputOptions _upscaleOptions;

        public TextToImageView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<TextToImageView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            DiffusionService = diffusionService;
            UpscaleService = upscaleService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.TextToImage;
        public IDiffusionService DiffusionService { get; }
        public IUpscaleService UpscaleService { get; }
        public AsyncRelayCommand ExecuteCommand { get; set; }
        public AsyncRelayCommand CancelCommand { get; set; }

        public ImageInput ResultImage
        {
            get { return _resultImage; }
            set { SetProperty(ref _resultImage, value); }
        }

        public ImageInput CompareImage
        {
            get { return _compareImage; }
            set { SetProperty(ref _compareImage, value); }
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
                _logger?.LogInformation($"[TextToImageView] [LoadPipelineAsync] - Loading pipeline...");
                await base.LoadPipelineAsync();

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
                _logger?.LogInformation($"[TextToImageView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[TextToImageView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[TextToImageView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[TextToImageView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[TextToImageView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                if (DiffusionService.IsLoaded)
                    await DiffusionService.UnloadAsync();
                if (UpscaleService.IsLoaded)
                    await UpscaleService.UnloadAsync();
                _logger?.LogInformation($"[TextToImageView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[TextToImageView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                var previousImage = _resultImage;
                Progress.Clear();
                Statistics.Clear();
                ResultImage = default;
                CompareImage = default;
                _logger?.LogInformation($"[TextToImageView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Diffusion
                var options = _options with { Strength = 1 };
                var resultTensor = await DiffusionService.GenerateImageAsync(options);

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    Progress.Indeterminate("Upscaling Image...");
                    resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                    {
                        Image = resultTensor,
                        Options = _upscaleOptions
                    });
                }

                Statistics.Stop();

                // Set Result
                ResultImage = await resultTensor.ToImageInputAsync();
                CompareImage = previousImage;

                // History
                await HistoryService.AddAsync(ResultImage, new DiffusionHistory
                {
                    Options = options,
                    Model = CurrentPipeline.DiffusionModel.Name,
                    LoraModel = CurrentPipeline.LoraAdapterModel?.Name,
                    UpscaleModel = CurrentPipeline.UpscaleModel?.Name,
                    UpscaleOptions = CurrentPipeline.UpscaleModel is not null ? _upscaleOptions : null,
                    Source = View.TextToImage,
                });

                _logger?.LogInformation($"[TextToImageView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[TextToImageView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[TextToImageView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[TextToImageView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
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