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
    /// Interaction logic for ImageExtractView.xaml
    /// </summary>
    public partial class ImageExtractView : ViewBase
    {
        private readonly ILogger _logger;
        private ImageInput _sourceImage;
        private ImageInput _resultImage;
        private ImageInput _compareImage;
        private ExtractInputOptions _options;

        public ImageExtractView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, IExtractService extractService, ILogger<ImageExtractView> logger)
            : base(settings, navigationService, environmentService, historyService)
        {
            _logger = logger;
            ExtractService = extractService;
            ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, CanExecute);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanCancel);
            Options = new ExtractInputOptions
            {
                TileSize = 512,
                TileOverlap = 16,
                IsInverted = false,
                MergeInput = false,
                Mode = TensorStack.Extractors.Common.BackgroundMode.RemoveBackground,
                Detections = 0,
                BodyConfidence = 0.4f,
                JointConfidence = 0.1f,
                ColorAlpha = 0.8f,
                JointRadius = 7f,
                BoneRadius = 8f,
                BoneThickness = 1f,
                IsTransparent = false
            };
            InitializeComponent();
        }

        public override int Id => (int)View.ImageExtract;
        public IExtractService ExtractService { get; }
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

        public ExtractInputOptions Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            if (ExtractService.IsLoaded)
            {
                //SelectedModel = ExtractService.Model;
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
                _logger?.LogInformation($"[ImageExtractView] [LoadPipelineAsync] - Loading pipeline...");

                await base.LoadPipelineAsync();
                await ExtractService.UnloadAsync();

                if (CurrentPipeline.ExtractModel is not null)
                    await ExtractService.LoadAsync(CurrentPipeline);

                SetDefaultOptions(ExtractService.DefaultOptions);
                await Settings.SetDefaultsAsync(CurrentPipeline);
                _logger?.LogInformation($"[ImageExtractView] [LoadPipelineAsync] - Loading pipeline complete.");
                IsPipelineLoaded = true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"[ImageExtractView] [LoadPipelineAsync] - Loading pipeline cancelled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageExtractView] [LoadPipelineAsync] - An exception occurred loading pipeline.");
                await DialogService.ShowErrorAsync("LoadPipelineAsync", ex.Message);
            }

            Progress.Clear();
            Statistics.Clear();
            _logger?.LogInformation($"[ImageExtractView] [LoadPipelineAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        protected override async Task UnloadPipelineAsync()
        {
            try
            {
                _logger?.LogInformation($"[ImageExtractView] [UnloadPipelineAsync] - Unloading pipeline...");
                await base.UnloadPipelineAsync();
                await ExtractService.UnloadAsync();
                _logger?.LogInformation($"[ImageExtractView] [UnloadPipelineAsync] -  Pipeline unloaded.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImageExtractView] [UnloadPipelineAsync] - An exception occurred unloading pipeline.");
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
                _logger?.LogInformation($"[ImageExtractView] [ExecuteAsync] - Executing pipeline..");

                Statistics.Start();

                // Run Extractor
                var resultTensor = await ExtractService.ExecuteAsync(new ExtractImageRequest
                {
                    Image = _sourceImage,
                    Options = _options,
                });

                Statistics.Stop();

                // Set Result
                ResultImage = await resultTensor.ToImageInputAsync();
                CompareImage = _sourceImage;

                // History
                await HistoryService.AddAsync(_resultImage, new ExtractHistory
                {
                    Options = _options,
                    Model = CurrentPipeline.ExtractModel.Name,
                    ExtractorType = CurrentPipeline.ExtractModel.Type,
                    Source = View.ImageExtract,
                });

                _logger?.LogInformation($"[ImageExtractView] [ExecuteAsync] - Executing pipeline complete.");
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                _logger?.LogInformation($"[ImageExtractView] [ExecuteAsync] - Executing pipeline cancelled.");
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                _logger?.LogError(ex, $"[ImageExtractView] [ExecuteAsync] - An exception occurred executing pipeline.");
                await DialogService.ShowErrorAsync("ExecuteAsync", ex.Message);
            }

            Progress.Clear();
            _logger?.LogInformation($"[ImageExtractView] [ExecuteAsync] - Elapsed: {Stopwatch.GetElapsedTime(timestamp)}");
        }


        private bool CanExecute()
        {
            return _sourceImage is not null && ExtractService.IsLoaded && !ExtractService.IsExecuting;
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

        private void SetDefaultOptions(ExtractInputOptions options)
        {
            Options = new ExtractInputOptions
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