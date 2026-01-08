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
    /// Interaction logic for ImageToImageView.xaml
    /// </summary>
    public partial class ImageToImageView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private ImageInput _sourceImage;
        private DiffusionInputOptions _options;
        private ImageInput _extractImage;
        private UpscaleInputOptions _upscaleOptions;
        private ExtractInputOptions _extractOptions;

        public ImageToImageView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IExtractService extractService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<ImageToImageView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            UpscaleService = upscaleService;
            ExtractService = extractService;
            DiffusionService = diffusionService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.ImageToImage;
        public IDiffusionService DiffusionService { get; }
        public IUpscaleService UpscaleService { get; }
        public IExtractService ExtractService { get; }
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

        public ExtractInputOptions ExtractOptions
        {
            get { return _extractOptions; }
            set { SetProperty(ref _extractOptions, value); }
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
                _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Loading pipeline..");

                await base.LoadPipelineAsync();
                await UnloadServicesAsync();

                if (CurrentPipeline.DiffusionModel is not null)
                {
                    await DiffusionService.LoadAsync(CurrentPipeline, PythonProgressCallback);
                    SetDefaultOptions(DiffusionService.DefaultOptions);
                }
                if (CurrentPipeline.ExtractModel is not null)
                {
                    await ExtractService.LoadAsync(CurrentPipeline);
                    SetDefaultOptions(ExtractService.DefaultOptions);
                }
                if (CurrentPipeline.UpscaleModel is not null)
                {
                    await UpscaleService.LoadAsync(CurrentPipeline);
                    SetDefaultOptions(UpscaleService.DefaultOptions);
                }

                SetDefaultOptions(DiffusionService.DefaultOptions);
                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Loading pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageToImageView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[ImageToImageView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await UnloadServicesAsync();
                _logger?.LogInformation($"[ImageToImageView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageToImageView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                ResultImage = default;
                CompareImage = default;
                _logger?.LogInformation($"[ImageToImageView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Diffusion
                var options = _options with
                {
                    InputImage = _sourceImage
                };
                var resultTensor = await DiffusionService.GenerateImageAsync(options);

                Statistics.Stop();

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                    {
                        Image = resultTensor,
                        Options = _upscaleOptions
                    });
                }

                // Set Result
                CompareImage = _sourceImage;
                ResultImage = new ImageInput(resultTensor);

                // History
                await HistoryService.AddAsync(ResultImage, new DiffusionHistory
                {
                    Options = options,
                    Model = CurrentPipeline.DiffusionModel.Name,
                    LoraModel = CurrentPipeline.LoraAdapterModel?.Name,
                    UpscaleModel = CurrentPipeline.UpscaleModel?.Name,
                    UpscaleOptions = CurrentPipeline.UpscaleModel is not null ? _upscaleOptions : null,
                    ExtractModel = CurrentPipeline.ExtractModel?.Name,
                    ExtractOptions = CurrentPipeline.ExtractModel is not null ? _extractOptions : null,
                    Source = View.ImageToImage,
                });

                _logger?.LogInformation($"[ImageToImageView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[ImageToImageView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[ImageToImageView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ImageToImageView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
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


        private void SetDefaultOptions(DiffusionDefaultOptions options)
        {
            Options = new DiffusionInputOptions
            {
                Prompt = Options?.Prompt,
                NegativePrompt = Options?.NegativePrompt,
                Seed = Options?.Seed ?? 0,
                LoraStrength = 1,
                Strength = 0.65f,
                ControlNetStrength = 1,
                Width = options.Width,
                Height = options.Height,
                Steps = options.Steps,
                Scheduler = options.Scheduler,
                GuidanceScale = options.GuidanceScale
            };
        }


        private void SetDefaultOptions(UpscaleInputOptions options)
        {
            UpscaleOptions = new UpscaleInputOptions
            {
                TileMode = options.TileMode,
                TileSize = options.TileSize,
                TileOverlap = options.TileOverlap,
            };
        }


        private void SetDefaultOptions(ExtractInputOptions options)
        {
            ExtractOptions = new ExtractInputOptions
            {
                TileMode = options.TileMode,
                TileSize = options.TileSize,
                TileOverlap = options.TileOverlap,
                IsInverted = options.IsInverted,
                IsTransparent = options.IsTransparent,
                MergeInput = options.MergeInput,
                Mode = options.Mode,
                Detections = options.Detections,
                BodyConfidence = options.BodyConfidence,
                JointConfidence = options.JointConfidence,
                ColorAlpha = options.ColorAlpha,
                JointRadius = options.JointRadius,
                BoneRadius = options.BoneRadius,
                BoneThickness = options.BoneThickness
            };
        }


        private async Task UnloadServicesAsync()
        {
            if (DiffusionService.IsLoaded)
                await DiffusionService.UnloadAsync();
            if (ExtractService.IsLoaded)
                await ExtractService.UnloadAsync();
            if (UpscaleService.IsLoaded)
                await UpscaleService.UnloadAsync();
        }


        protected async void SourceImage_SourceChanged(object sender, ImageInput image)
        {
            if (ExtractService.IsLoaded)
            {
                try
                {
                    if (_extractImage == _sourceImage)
                        return;

                    if (_sourceImage == null)
                        return;

                    IsViewBusy = true;
                    Progress.Indeterminate("Extracting Image Features...");
                    _extractImage = new ImageInput(await ExtractService.ExecuteAsync(new ExtractImageRequest
                    {
                        Image = _sourceImage,
                        Options = _extractOptions
                    }));
                    SourceImage = _extractImage;
                    Progress.Clear();
                }
                finally
                {
                    IsViewBusy = false;
                }
            }
        }
    }
}