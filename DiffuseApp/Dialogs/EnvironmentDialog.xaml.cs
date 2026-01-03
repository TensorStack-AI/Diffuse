// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Services;
using System;
using System.Threading.Tasks;
using TensorStack.Python.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for EnvironmentDialog.xaml
    /// </summary>
    public partial class EnvironmentDialog : DialogControl
    {
        private readonly IEnvironmentService _environmentService;
        private readonly IProgress<PipelineProgress> _progressCallback;
        private bool _isExecuting;

        public EnvironmentDialog(IEnvironmentService environmentService)
        {
            _environmentService = environmentService;
            _progressCallback = new Progress<PipelineProgress>(OnProgressUpdate);
            NoCommand = new AsyncRelayCommand(CloseAsync);
            YesCommand = new AsyncRelayCommand(Yes);
            RebuildCommand = new AsyncRelayCommand(Rebuild);
            Progress = new ProgressInfo();
            Exists = _environmentService.Exists();
            InitializeComponent();
        }

        public AsyncRelayCommand NoCommand { get; }
        public AsyncRelayCommand YesCommand { get; }
        public AsyncRelayCommand RebuildCommand { get; }
        public ProgressInfo Progress { get; set; }
        public bool Exists { get; set; }

        public bool IsExecuting
        {
            get { return _isExecuting; }
            set { SetProperty(ref _isExecuting, value); }
        }


        private async Task Yes()
        {
            IsExecuting = true;
            await Task.Run(() => _environmentService.CreateAsync(false, false, _progressCallback));
            await base.SaveAsync();
        }


        private async Task Rebuild()
        {
            IsExecuting = true;
            await Task.Run(() => _environmentService.CreateAsync(true, false, _progressCallback));
            await base.SaveAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private void OnProgressUpdate(PipelineProgress progress)
        {
            Progress.Indeterminate(progress.Message);
        }

    }
}
