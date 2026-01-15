using Diffuse.Common;
using Diffuse.Dialogs;
using Diffuse.Services;
using DiffuseApp.Common;
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
        private bool _isViewBusy;
        private PipelineModel _currentPipeline;
        private bool _isPipelineLoaded;
        private readonly Dictionary<string, PipelineProgress> _downloadStatistics;

        public ViewBase(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(navigationService)
        {
            Settings = settings;
            EnvironmentService = environmentService;
            HistoryService = historyService;
            Progress = new ProgressInfo();
            Statistics = new StatisticsModel(Dispatcher);
            ProgressCallback = new Progress<RunProgress>(OnProgress);
            PythonProgressCallback = new Progress<PipelineProgress>(OnProgress);
            _downloadStatistics = new Dictionary<string, PipelineProgress>();
        }

        public Settings Settings { get; }
        public IHistoryService HistoryService { get; }
        public IEnvironmentService EnvironmentService { get; }
        public ProgressInfo Progress { get; }
        protected IProgress<RunProgress> ProgressCallback { get; }
        protected IProgress<PipelineProgress> PythonProgressCallback { get; }
        public StatisticsModel Statistics { get; }

        public bool IsViewBusy
        {
            get { return _isViewBusy; }
            set { SetProperty(ref _isViewBusy, value); }
        }

        public PipelineModel CurrentPipeline
        {
            get { return _currentPipeline; }
            set { SetProperty(ref _currentPipeline, value); }
        }

        public bool IsPipelineLoaded
        {
            get { return _isPipelineLoaded; }
            set { SetProperty(ref _isPipelineLoaded, value); }
        }



        protected virtual async Task<bool> LoadEnvironment()
        {
            if (EnvironmentService.Exists(_currentPipeline))
                return true;

            var environmentDialog = DialogService.GetDialog<EnvironmentDialog>();
            await environmentDialog.CreateAsync(_currentPipeline);
            return EnvironmentService.Exists(_currentPipeline);
        }


        protected virtual Task LoadPipelineAsync()
        {
            _downloadStatistics.Clear();
            return Task.CompletedTask;
        }


        protected virtual Task UnloadPipelineAsync()
        {
            return Task.CompletedTask;
        }


        protected virtual void OnProgress(RunProgress progress)
        {
            Progress.Update(progress.Value, progress.Maximum, progress.Message);
        }


        protected virtual void OnProgress(PipelineProgress progress)
        {
            if (_currentPipeline is null)
                return;

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
                Statistics.Update(progress);
                Progress.Update(progress.Iteration, progress.Iterations, $"Step: {progress.Iteration}/{progress.Iterations}");
            }
        }


        protected async void SelectedPipelineChanged(object sender, PipelineModel pipeline)
        {
            if (pipeline.DiffusionModel == null)
            {
                await UnloadPipelineAsync();
                CurrentPipeline = default;
            }
            else
            {
                if (await LoadEnvironment())
                {
                    if (pipeline.UpscaleModel is not null && !pipeline.UpscaleModel.IsValid)
                    {
                        if (!await pipeline.UpscaleModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Upscale")))
                            CurrentPipeline = default;
                    }
                    if (pipeline.ExtractModel is not null && !pipeline.ExtractModel.IsValid)
                    {
                        if (!await pipeline.ExtractModel.DownloadAsync(Path.Combine(Settings.DirectoryModel, "Extract")))
                            CurrentPipeline = default;
                    }
                }
                else
                {
                    CurrentPipeline = default;
                }

                if (CurrentPipeline is not null)
                    await LoadPipelineAsync();
            }
            await Task.Delay(500);
            Progress.Clear();
        }

    }
}
