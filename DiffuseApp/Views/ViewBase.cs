using Diffuse.Common;
using Diffuse.Dialogs;
using Diffuse.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Common.Pipeline;
using TensorStack.Python.Common;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    public abstract class ViewBase : ViewControl
    {
        private PipelineModel _currentPipeline;
        private Dictionary<string, PipelineProgress> _downloadStatistics;
        private readonly IEnvironmentService _environmentService;

        public ViewBase(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(navigationService)
        {
            Settings = settings;
            _environmentService = environmentService;
            HistoryService = historyService;
            Progress = new ProgressInfo();
            ProgressCallback = new Progress<RunProgress>(OnProgress);
            PythonProgressCallback = new Progress<PipelineProgress>(OnProgress);
            _downloadStatistics = new Dictionary<string, PipelineProgress>();
        }

        public Settings Settings { get; }
        public IHistoryService HistoryService { get; }
        public ProgressInfo Progress { get; }
        protected IProgress<RunProgress> ProgressCallback { get; }
        protected IProgress<PipelineProgress> PythonProgressCallback { get; }

        public PipelineModel CurrentPipeline
        {
            get { return _currentPipeline; }
            set { SetProperty(ref _currentPipeline, value); }
        }

        protected virtual async Task<bool> LoadEnvironment()
        {
            if (_environmentService.IsLoaded)
                return true;

            var environmentDialog = DialogService.GetDialog<EnvironmentDialog>();
            await environmentDialog.ShowDialogAsync();
            return _environmentService.IsLoaded;
        }

        protected virtual Task LoadPipelineAsync()
        {
            _downloadStatistics.Clear();
            return Task.CompletedTask;
        }


        protected virtual void OnProgress(RunProgress progress)
        {
            Progress.Update(progress.Value, progress.Maximum, progress.Message);
        }


        protected virtual void OnProgress(PipelineProgress progress)
        {
            if (progress.IsDownloading)
            {
                _downloadStatistics[progress.Message] = progress;
                var total = (int)_downloadStatistics.Values.Sum(x => x.DownloadTotal);
                var download = (int)_downloadStatistics.Values.Sum(x => x.Downloaded);
                Progress.Update(download, total, $"Downloading {_currentPipeline.DiffusionModel.Name}...");
                return;
            }
            else if (progress.IsLoading)
            {
                Progress.Update(progress.Iteration, progress.Iterations, $"Loading {_currentPipeline.DiffusionModel.Name}...");
            }
            else if (progress.IsGenerating)
            {
                Progress.Update(progress.Iteration, progress.Iterations, $"Step: {progress.Iteration}/{progress.Iterations}");
            }
        }


        protected async void SelectedPipelineChanged(object sender, PipelineModel pipeline)
        {
            if (await LoadEnvironment())
            {
                if (pipeline.UpscaleModel is not null && !pipeline.UpscaleModel.IsValid)
                {
                    if (!await pipeline.UpscaleModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Upscale"))) ;
                    CurrentPipeline = default;
                }
                if (pipeline.ExtractorModel is not null && !pipeline.ExtractorModel.IsValid)
                {
                    if (!await pipeline.ExtractorModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Extractor")))
                        CurrentPipeline = default;
                }
            }
            else
            {
                CurrentPipeline = default;
            }

            if (CurrentPipeline is not null)
                await LoadPipelineAsync();

            await Task.Delay(500);
            Progress.Clear();
        }

    }

}
