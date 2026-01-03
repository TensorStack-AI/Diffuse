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
    /// Interaction logic for ExtractorModelDialog.xaml
    /// </summary>
    public partial class ExtractorModelDialog : DialogControl
    {
        private ExtractorModel _extractorModel;
        private ExtractorModel _originalExtractorModel;
        private string _selectedFile;

        public ExtractorModelDialog(Settings settings)
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
        public bool IsUpdateMode => _originalExtractorModel is not null;

        public ExtractorModel ExtractorModel
        {
            get { return _extractorModel; }
            set { SetProperty(ref _extractorModel, value); }
        }

        public string SelectedFile
        {
            get { return _selectedFile; }
            set { SetProperty(ref _selectedFile, value); }
        }


        public Task<bool> AddAsync()
        {
            var modelId = GetNextModelId();
            ExtractorModel = new ExtractorModel
            {
                Id = modelId
            };
            return base.ShowDialogAsync();
        }


        public Task<bool> UpdateAsync(ExtractorModel extractorModel)
        {
            var modelId = extractorModel.Id;
            _originalExtractorModel = extractorModel;
            ExtractorModel = DeepClone(extractorModel, modelId);
            foreach (var path in ExtractorModel.UrlPaths)
            {
                Files.Add(path);
            }
            return base.ShowDialogAsync();
        }


        public Task<bool> CopyAsync(ExtractorModel extractorModel)
        {
            var modelId = GetNextModelId();
            ExtractorModel = DeepClone(extractorModel, modelId);
            foreach (var path in ExtractorModel.UrlPaths)
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
            var index = Settings.ExtractorModels.Count;
            if (IsUpdateMode)
            {
                index = Settings.ExtractorModels.IndexOf(_originalExtractorModel);
                Settings.ExtractorModels.Remove(_originalExtractorModel);
            }

            ExtractorModel.UrlPaths = Files.ToArray();
            Settings.ExtractorModels.Insert(index, ExtractorModel);
            return base.SaveAsync();
        }


        protected override Task CancelAsync()
        {
            ExtractorModel = default;
            _originalExtractorModel = null;
            return base.CancelAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private int GetNextModelId()
        {
            return Settings.ExtractorModels.Max(x => x.Id) + 1;
        }


        private static ExtractorModel DeepClone(ExtractorModel extractorModel, int modelId)
        {
            return new ExtractorModel
            {
                Id = modelId,
                Name = extractorModel.Name,
                Path = extractorModel.Path,
                Channels = extractorModel.Channels,
                IsDefault = extractorModel.IsDefault,
                Normalization = extractorModel.Normalization,
                OutputNormalization = extractorModel.OutputNormalization,
                SampleSize = extractorModel.SampleSize,
                OutputChannels = extractorModel.OutputChannels,
                IsDynamicOutput = extractorModel.IsDynamicOutput,
                Type = extractorModel.Type,
                UrlPaths = extractorModel.UrlPaths.ToArray(),
            };
        }
    }
}
