using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Common.Tensor;
using TensorStack.Image;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for ImageEditView.xaml
    /// </summary>
    public partial class ImageEditView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private ImageInput _sourceImage1;
        private ImageInput _sourceImage2;
        private ImageInput _sourceImage3;
        private ImageInput _sourceImage4;
        private GenerateOptions _options;

        public ImageEditView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IExtractorService extractorService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<ImageEditView> logger)
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

        public override int Id => (int)View.ImageEdit;
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

        public ImageInput SourceImage1
        {
            get { return _sourceImage1; }
            set { SetProperty(ref _sourceImage1, value); }
        }

        public ImageInput SourceImage2
        {
            get { return _sourceImage2; }
            set { SetProperty(ref _sourceImage2, value); }
        }

        public ImageInput SourceImage3
        {
            get { return _sourceImage3; }
            set { SetProperty(ref _sourceImage3, value); }
        }

        public ImageInput SourceImage4
        {
            get { return _sourceImage4; }
            set { SetProperty(ref _sourceImage4, value); }
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
                _logger?.LogInformation($"[ImageEditView] [LoadPipelineAsync] - Loading pipeline..");

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
                _logger?.LogInformation($"[ImageEditView] [LoadPipelineAsync] - Loading pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ImageEditView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageEditView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ImageEditView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private async Task ExecuteAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();

            try
            {
                Progress.Clear();
                CompareImage = default;
                _logger?.LogInformation($"[ImageEditView] [ExecuteAsync] - Executing pipeline..");

                var images = new List<ImageTensor> { _sourceImage1, _sourceImage2, _sourceImage3, _sourceImage4 };
                var options = _options with
                {
                    InputImages = [.. images.Where(x => x != null).Take(_options.InputImageCount)]
                };

                // Run Diffusion
                var resultTensor = await DiffusionService.GenerateImageAsync(options);

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                    {
                        Image = resultTensor
                    });
                }

                // Set Result
                CompareImage = _sourceImage1;
                ResultImage = new ImageInput(resultTensor);

                // History
                await HistoryService.AddAsync(ResultImage, View.ImageEdit, options);

                _logger?.LogInformation($"[ImageEditView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ImageEditView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageEditView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ImageEditView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
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
                GuidanceScale = options.GuidanceScale,
                InputImageCount = 1
            };
        }

    }
}