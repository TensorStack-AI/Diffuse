using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TensorStack.Image;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for ImageUpscaleView.xaml
    /// </summary>
    public partial class ImageUpscaleView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _sourceImage;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private UpscaleInputOptions _options;

        public ImageUpscaleView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, IUpscaleService upscaleService, ILogger<ImageUpscaleView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            UpscaleService = upscaleService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            InitializeComponent();
        }

        public override int Id => (int)View.ImageUpscale;
        public IUpscaleService UpscaleService { get; }
        public AsyncRelayCommand ExecuteCommand { get; set; }
        public AsyncRelayCommand CancelCommand { get; set; }

        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { SetProperty(ref _sourceImage, value); }
        }

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

        public UpscaleInputOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            if (UpscaleService.IsLoaded)
            {
                // SelectedModel = UpscaleService.Model;
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
                _logger?.LogInformation($"[ImageUpscaleView] [LoadPipelineAsync] - Loading pipeline...");

                await base.LoadPipelineAsync();
                await UpscaleService.UnloadAsync();

                if (CurrentPipeline.UpscaleModel is not null)
                    await UpscaleService.LoadAsync(CurrentPipeline);

                SetDefaultOptions(UpscaleService.DefaultOptions);
                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[ImageUpscaleView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ImageUpscaleView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageUpscaleView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[ImageUpscaleView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[ImageUpscaleView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await UpscaleService.UnloadAsync();
                _logger?.LogInformation($"[ImageUpscaleView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageUpscaleView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                _logger?.LogInformation($"[ImageUpscaleView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Upscaler
                var resultTensor = await UpscaleService.ExecuteAsync(new UpscaleImageRequest
                {
                    Image = _sourceImage,
                    Options = _options
                });

                Statistics.Stop();

                // Set Result
                ResultImage = await resultTensor.ToImageInputAsync();
                CompareImage = _sourceImage;

                // History
                await HistoryService.AddAsync(_resultImage, new UpscaleHistory
                {
                    Options = _options,
                    Model = CurrentPipeline.UpscaleModel.Name,
                    Source = View.ImageUpscale,
                    OriginalWidth = _sourceImage.Width,
                    OriginalHeight = _sourceImage.Height,
                    ScaleFactor = CurrentPipeline.UpscaleModel.ScaleFactor
                });

                _logger?.LogInformation($"[ImageUpscaleView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[ImageUpscaleView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[ImageUpscaleView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ImageUpscaleView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return _sourceImage is not null && UpscaleService.IsLoaded && !UpscaleService.IsExecuting;
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


        private void SetDefaultOptions(UpscaleInputOptions options)
        {
            Options = new UpscaleInputOptions
            {
                TileMode = options.TileMode,
                TileSize = options.TileSize,
                TileOverlap = options.TileOverlap,
            };
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