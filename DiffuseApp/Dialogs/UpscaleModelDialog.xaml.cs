// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for UpscaleModelDialog.xaml
    /// </summary>
    public partial class UpscaleModelDialog : DialogControl
    {
        private UpscaleModel _upscaleModel;
        private UpscaleModel _originalUpscaleModel;
        private string _selectedFile;

        public UpscaleModelDialog(Settings settings)
        {
            Settings = settings;
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            AddFileCommand = new AsyncRelayCommand(AddFileAsync, CanAddFile);
            RemoveFileCommand = new AsyncRelayCommand<string>(RemoveFileAsync);
            Files = new ObservableCollection<string>();
            InitializeComponent();
        }

        public Settings Settings { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand AddFileCommand { get; }
        public AsyncRelayCommand<string> RemoveFileCommand { get; }
        public ObservableCollection<string> Files { get; }
        public bool IsUpdateMode => _originalUpscaleModel is not null;

        public UpscaleModel UpscaleModel
        {
            get { return _upscaleModel; }
            set { SetProperty(ref _upscaleModel, value); }
        }

        public string SelectedFile
        {
            get { return _selectedFile; }
            set { SetProperty(ref _selectedFile, value); }
        }


        public Task<bool> AddAsync()
        {
            var modelId = GetNextModelId();
            UpscaleModel = new UpscaleModel
            {
                Id = modelId
            };
            return base.ShowDialogAsync();
        }


        public Task<bool> UpdateAsync(UpscaleModel upscaleModel)
        {
            var modelId = upscaleModel.Id;
            _originalUpscaleModel = upscaleModel;
            UpscaleModel = DeepClone(upscaleModel, modelId);
            foreach (var path in UpscaleModel.UrlPaths)
            {
                Files.Add(path);
            }
            return base.ShowDialogAsync();
        }


        public Task<bool> CopyAsync(UpscaleModel upscaleModel)
        {
            var modelId = GetNextModelId();
            UpscaleModel = DeepClone(upscaleModel, modelId);
            foreach (var path in UpscaleModel.UrlPaths)
            {
                Files.Add(path);
            }
            return base.ShowDialogAsync();
        }


        private Task AddFileAsync()
        {
            Files.Add(SelectedFile);
            SelectedFile = null;
            return Task.CompletedTask;
        }


        private bool CanAddFile()
        {
            return !Files.Any(x => x == SelectedFile);
        }


        private Task RemoveFileAsync(string file)
        {
            Files.Remove(file);
            SelectedFile = null;
            return Task.CompletedTask;
        }


        protected override Task SaveAsync()
        {
            var index = Settings.UpscaleModels.Count;
            if (IsUpdateMode)
            {
                index = Settings.UpscaleModels.IndexOf(_originalUpscaleModel);
                Settings.UpscaleModels.Remove(_originalUpscaleModel);
            }

            UpscaleModel.UrlPaths = Files.ToArray();
            Settings.UpscaleModels.Insert(index, UpscaleModel);
            return base.SaveAsync();
        }


        protected override Task CancelAsync()
        {
            UpscaleModel = default;
            _originalUpscaleModel = null;
            return base.CancelAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private int GetNextModelId()
        {
            return Settings.UpscaleModels.Max(x => x.Id) + 1;
        }


        private static UpscaleModel DeepClone(UpscaleModel upscaleModel, int modelId)
        {
            return new UpscaleModel
            {
                Id = modelId,
                Name = upscaleModel.Name,
                Path = upscaleModel.Path,
                Channels = upscaleModel.Channels,
                IsDefault = upscaleModel.IsDefault,
                Normalization = upscaleModel.Normalization,
                OutputNormalization = upscaleModel.OutputNormalization,
                SampleSize = upscaleModel.SampleSize,
                ScaleFactor = upscaleModel.ScaleFactor,
                UrlPaths = upscaleModel.UrlPaths.ToArray(),
            };
        }
    }
}
