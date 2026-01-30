using Diffuse.Common;
using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Image;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for ControlNetImageToImageView.xaml
    /// </summary>
    public partial class ControlNetImageToImageView : ViewBaseDiffusion
    {
        private ImageInput _sourceImage;
        private ImageInput _controlImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlNetImageToImageView"/> class.
        /// </summary>
        public ControlNetImageToImageView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IDiffusionService diffusionService, IExtractService extractService, IUpscaleService upscaleService, IHistoryService historyService, ILogger<ControlNetImageToImageView> logger)
            : base(settings, navigationService, environmentService, diffusionService, extractService, upscaleService, historyService, logger)
        {
            InitializeComponent();
        }

        public override View View => View.ControlNetImageToImage;

        /// <summary>
        /// Gets or sets the source image.
        /// </summary>
        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { SetProperty(ref _sourceImage, value); }
        }

        /// <summary>
        /// Gets or sets the control image.
        /// </summary>
        public ImageInput ControlImage
        {
            get { return _controlImage; }
            set { SetProperty(ref _controlImage, value); }
        }


        /// <summary>
        /// Execute thge pipeline.
        /// </summary>
        protected override async Task ExecuteAsync()
        {
            var timestamp = Stopwatch.GetTimestamp();
            Logger.LogInformation($"[ControlNetImageToImage] [Execute] Executing pipeline...");

            try
            {
                Progress.Clear();
                Statistics.Clear();
                ResultImage = default;
                CompareImage = default;
                Statistics.Start();

                // Diffusion
                var options = Options with
                {
                    InputImage = _sourceImage,
                    InputControlImage = _controlImage
                };
                var resultTensor = await ExecuteImageDiffusionAsync(options);

                // Upscale
                resultTensor = await ExecuteImageUpscaleAsync(resultTensor);

                // Result
                Statistics.Stop();
                ResultImage = await resultTensor.ToImageInputAsync();
                CompareImage = _sourceImage;

                // History
                await SaveHistoryAsync(options);
                Logger.LogInformation("[ControlNetImageToImage] [Execute] Executing pipeline complete, Elapsed: {Elapsed:c}", Stopwatch.GetElapsedTime(timestamp));
            }
            catch (OperationCanceledException)
            {
                Statistics.Clear();
                Logger.LogInformation("[ControlNetImageToImage] [Execute] Executing pipeline cancelled, Elapsed: {Elapsed:c}", Stopwatch.GetElapsedTime(timestamp));
            }
            catch (Exception ex)
            {
                Statistics.Clear();
                Logger.LogError(ex, "[ControlNetImageToImage] [Execute] An exception occurred executing pipeline, Elapsed: {Elapsed:c}", Stopwatch.GetElapsedTime(timestamp));
                await DialogService.ShowErrorAsync("Execute Pipeline", ex.Message);
            }
            finally
            {
                Progress.Clear();
            }
        }


        /// <summary>
        /// Unloads the pipeline
        /// </summary>
        protected override Task<bool> UnloadPipelineAsync()
        {
            return base.UnloadPipelineAsync();
        }


        /// <summary>
        /// Called when SourceImage changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="image">The image.</param>
        protected async void OnSourceImageChanged(object sender, ImageInput image)
        {
            try
            {
                if (!ExtractService.IsLoaded)
                    return;

                IsViewBusy = true;
                if (_sourceImage == null)
                {
                    ControlImage = null;
                    return;
                }

                ControlImage =  await ExecuteImageExtractAsync(_sourceImage);
            }
            finally
            {
                Progress.Clear();
                IsViewBusy = false;
            }
        }


        /// <summary>
        /// Save history
        /// </summary>
        /// <param name="options">The options.</param>
        private async Task<ImageInput> SaveHistoryAsync(DiffusionInputOptions options)
        {
            Logger.LogInformation($"[ControlNetImageToImage] [SaveHistory] Saving history...");
            var result = await HistoryService.AddAsync(ResultImage, new DiffusionHistory
            {
                Options = options,
                Model = CurrentPipeline.DiffusionModel.Name,
                LoraModels = CurrentPipeline.LoraAdapterModel?.Select(x => x.Name).ToArray(),
                ControlNetModel = CurrentPipeline.ControlNetModel.Name,
                UpscaleModel = CurrentPipeline.UpscaleModel?.Name,
                UpscaleOptions = CurrentPipeline.UpscaleModel is not null ? UpscaleOptions : null,
                ExtractModel = CurrentPipeline.ExtractModel?.Name,
                ExtractorType = CurrentPipeline.ExtractModel?.Type,
                ExtractOptions = CurrentPipeline.ExtractModel is not null ? ExtractOptions : null,
                Source = View.ControlNetImageToImage,
            });
            Logger.LogInformation($"[ControlNetImageToImage] [SaveHistory] History saved.");
            return result;
        }
    }
}