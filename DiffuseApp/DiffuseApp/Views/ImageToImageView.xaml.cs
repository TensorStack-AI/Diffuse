using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
                IsPipelineLoaded = false;
                Progress.Indeterminate("Loading Pipeline...");
                _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Loading pipeline..");
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

                //ExtractModel
                if (CurrentPipeline.ExtractModel is not null)
                {
                    if (!ExtractService.IsLoaded || ExtractService.Pipeline.ExtractModel != CurrentPipeline.ExtractModel)
                    {
                        await ExtractService.LoadAsync(CurrentPipeline);
                    }
                }
                else
                {
                    await ExtractService.UnloadAsync();
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
                _logger?.LogInformation($"[ImageToImageView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
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
                if (DiffusionService.IsLoaded)
                    await DiffusionService.UnloadAsync();
                if (ExtractService.IsLoaded)
                    await ExtractService.UnloadAsync();
                if (UpscaleService.IsLoaded)
                    await UpscaleService.UnloadAsync();
                _logger?.LogInformation($"[ImageToImageView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageToImageView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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

                // Run Upscaler
                if (UpscaleService.IsLoaded)
                {
                    Progress.Indeterminate("Upscaling Image...");
                    resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                    {
                        Image = resultTensor,
                        Options = _upscaleOptions
                    }, ProgressCallback);
                }

                Statistics.Stop();

                // Set Result
                ResultImage = await resultTensor.ToImageInputAsync();
                CompareImage = _sourceImage;

                // History
                await HistoryService.AddAsync(ResultImage, new DiffusionHistory
                {
                    Options = options,
                    Model = CurrentPipeline.DiffusionModel.Name,
                    LoraModels = CurrentPipeline.LoraAdapterModel?.Select(x => x.Name).ToArray(),
                    UpscaleModel = CurrentPipeline.UpscaleModel?.Name,
                    UpscaleOptions = CurrentPipeline.UpscaleModel is not null ? _upscaleOptions : null,
                    ExtractModel = CurrentPipeline.ExtractModel?.Name,
                    ExtractorType = CurrentPipeline.ExtractModel?.Type,
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
                    var resultTensor = await ExtractService.ExecuteAsync(new ExtractImageRequest
                    {
                        Image = _sourceImage,
                        Options = _extractOptions
                    });
                    _extractImage = await resultTensor.ToImageInputAsync();
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