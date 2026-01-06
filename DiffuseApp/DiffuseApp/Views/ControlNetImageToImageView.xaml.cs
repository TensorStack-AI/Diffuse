using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TensorStack.Common.Tensor;
using TensorStack.Image;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for ControlNetImageToImageView.xaml
    /// </summary>
    public partial class ControlNetImageToImageView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private ImageInput _sourceImage;
        private GenerateOptions _options;

        public ControlNetImageToImageView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IExtractorService extractorService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<ControlNetImageToImageView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            UpscaleService = upscaleService;
            ExtractorService = extractorService;
            DiffusionService = diffusionService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.ControlNetImageToImage;
        public IDiffusionService DiffusionService { get; }
        public IUpscaleService UpscaleService { get; }
        public IExtractorService ExtractorService { get; }
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

        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { SetProperty(ref _sourceImage, value); }
        }

        public GenerateOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
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
                Progress.Indeterminate("Loading Pipeline...");
                _logger?.LogInformation($"[ControlNetImageToImageView] [LoadPipelineAsync] - Loading pipeline..");

                await base.LoadPipelineAsync();
                if (CurrentPipeline.DiffusionModel == null)
                    await DiffusionService.UnloadAsync();
                if (CurrentPipeline.ExtractorModel == null)
                    await ExtractorService.UnloadAsync();
                if (CurrentPipeline.UpscaleModel == null)
                    await UpscaleService.UnloadAsync();

                if (CurrentPipeline.DiffusionModel is not null)
                {
                    await DiffusionService.LoadAsync(CurrentPipeline, PythonProgressCallback);
                    SetDefaultOptions(DiffusionService.DefaultOptions);
                }

                if (CurrentPipeline.ExtractorModel is not null)
                {
                    await ExtractorService.LoadAsync(CurrentPipeline);
                }

                if (CurrentPipeline.UpscaleModel is not null)
                {
                    await UpscaleService.LoadAsync(CurrentPipeline);
                }

                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[TextToImageView] [LoadPipelineAsync] - Loading pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ControlNetImageToImageView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ControlNetImageToImageView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ControlNetImageToImageView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private async Task ExecuteAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();

            try
            {
                Progress.Clear();
                CompareImage = default;
                _logger?.LogInformation($"[ControlNetImageToImageView] [ExecuteAsync] - Executing pipeline..");

                var sourceImage = _sourceImage;
                var extractorImage = default(ImageTensor);

                // Run Extractor
                if (ExtractorService.IsLoaded)
                {
                    extractorImage = await ExtractorService.ExecuteAsync(new ExtractorImageRequest
                    {
                        Image = sourceImage
                    });
                }

                // Run Diffusion
                var resultTensor = await DiffusionService.GenerateImageAsync(_options with
                {
                    InputImage = sourceImage,
                    InputControlImage = extractorImage
                });

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                    {
                        Image = resultTensor
                    });
                }

                // Set Result
                CompareImage = _sourceImage;
                ResultImage = new ImageInput(resultTensor);

                // History
                await HistoryService.AddAsync(ResultImage, View.ControlNetImageToImage, _options);

                _logger?.LogInformation($"[ControlNetImageToImageView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ControlNetImageToImageView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ControlNetImageToImageView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ControlNetImageToImageView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return DiffusionService.IsLoaded && !DiffusionService.IsExecuting;
        }


        private async Task CancelAsync()
        {
            await DiffusionService.CancelAsync();
        }


        private bool CanCancel()
        {
            return DiffusionService.CanCancel;
        }


        private void SetDefaultOptions(DiffusionDefaultOptions options)
        {
            Options = new GenerateOptions
            {
                Prompt = Options?.Prompt,
                NegativePrompt = Options?.NegativePrompt,
                Seed = Options?.Seed ?? 0,
                Strength = 1,
                ControlNetStrength = 1,
                Width = options.Width,
                Height = options.Height,
                Steps = options.Steps,
                Scheduler = options.Scheduler,
                GuidanceScale = options.GuidanceScale
            };
        }

    }
}