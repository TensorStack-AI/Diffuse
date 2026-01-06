// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for LoraModelDialog.xaml
    /// </summary>
    public partial class LoraModelDialog : DialogControl
    {
        private LoraAdapterModel _loraModel;
        private LoraAdapterModel _originalLoraModel;
        private string _selectedTrigger;

        public LoraModelDialog(Settings settings)
        {
            Settings = settings;
            Trigger = new ObservableCollection<string>();
            Pipelines = new ObservableCollection<string>(GetPipelines());
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            AddTriggerCommand = new AsyncRelayCommand(AddTriggerAsync, CanAddTrigger);
            RemoveTriggerCommand = new AsyncRelayCommand<string>(RemoveTriggerAsync);
            Errors = new ObservableCollection<string>();
            InitializeComponent();
        }


        public Settings Settings { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public ObservableCollection<string> Errors { get; }
        public AsyncRelayCommand AddTriggerCommand { get; }
        public AsyncRelayCommand<string> RemoveTriggerCommand { get; }
        public ObservableCollection<string> Trigger { get; }
        public ObservableCollection<string> Pipelines { get; }
        public bool IsUpdateMode => _originalLoraModel is not null;

        public LoraAdapterModel LoraModel
        {
            get { return _loraModel; }
            set { SetProperty(ref _loraModel, value); }
        }

        public string SelectedTrigger
        {
            get { return _selectedTrigger; }
            set { SetProperty(ref _selectedTrigger, value); }
        }


        public Task<bool> AddAsync()
        {
            var modelId = GetNextModelId();
            LoraModel = new LoraAdapterModel
            {
                Id = modelId,
                Pipeline = Pipelines.First()
            };
            return base.ShowDialogAsync();
        }


        public Task<bool> UpdateAsync(LoraAdapterModel loraModel)
        {
            var modelId = loraModel.Id;
            _originalLoraModel = loraModel;
            LoraModel = DeepClone(loraModel, modelId);
            Populate();
            return base.ShowDialogAsync();
        }


        public Task<bool> CopyAsync(LoraAdapterModel loraModel)
        {
            var modelId = GetNextModelId();
            LoraModel = DeepClone(loraModel, modelId);
            LoraModel.Name += " copy";
            Populate();
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            var index = Settings.LoraAdapterModels.Count;
            if (IsUpdateMode)
            {
                index = Settings.LoraAdapterModels.IndexOf(_originalLoraModel);
                Settings.LoraAdapterModels.Remove(_originalLoraModel);
            }

            LoraModel.Key = CreateKey();
            LoraModel.Triggers = Trigger.Count == 0 ? default : Trigger.ToArray();
            Settings.LoraAdapterModels.Insert(index, LoraModel);
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            if (LoraModel == null)
                return false;

            Errors.Clear();
            foreach (var inputError in GetValidationErrors())
                Errors.Add(inputError);

            return Errors.Count == 0 && base.CanExecuteSave();
        }


        protected override Task CancelAsync()
        {
            LoraModel = default;
            _originalLoraModel = null;
            return base.CancelAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private Task AddTriggerAsync()
        {
            Trigger.Add(_selectedTrigger);
            SelectedTrigger = null;
            return Task.CompletedTask;
        }


        private bool CanAddTrigger()
        {
            return !Trigger.Contains(_selectedTrigger);
        }


        private Task RemoveTriggerAsync(string trigger)
        {
            Trigger.Remove(trigger);
            SelectedTrigger = null;
            return Task.CompletedTask;
        }


        private int GetNextModelId()
        {
            return Math.Max(100, Settings.LoraAdapterModels.Max(x => x.Id)) + 1;
        }


        private IEnumerable<string> GetPipelines()
        {
            var pipelines = new List<string>(["ChromaPipeline", "QwenImagePipeline", "WanPipeline", "ZImagePipeline"]);
            pipelines.AddRange(Settings.DiffusionModels.Select(x => x.Pipeline));
            return pipelines.Distinct();
        }


        private void Populate()
        {
            if (!LoraModel.Triggers.IsNullOrEmpty())
            {
                foreach (var trigger in LoraModel.Triggers)
                {
                    Trigger.Add(trigger);
                }
            }
        }

        private IEnumerable<string> GetValidationErrors()
        {
            if (string.IsNullOrWhiteSpace(LoraModel.Name))
                yield return "Name cannot be empty";
            if (string.IsNullOrWhiteSpace(LoraModel.Path))
                yield return "Path cannot be empty";
            if (string.IsNullOrWhiteSpace(LoraModel.Weights))
                yield return "Weights cannot be empty";
            if (string.IsNullOrWhiteSpace(LoraModel.Pipeline))
                yield return "Pipeline cannot be empty";
        }


        private static LoraAdapterModel DeepClone(LoraAdapterModel loraModel, int modelId)
        {
            return new LoraAdapterModel
            {
                Id = modelId,
                Name = loraModel.Name,
                Key = loraModel.Key,
                Path = loraModel.Path,
                Pipeline = loraModel.Pipeline,
                Weights = loraModel.Weights,
                Triggers = loraModel.Triggers,
            };
        }

        private string CreateKey()
        {
            return $"{new string([.. LoraModel.Name.Where(char.IsLetterOrDigit)])}{LoraModel.Id}".ToLower();
        }
    }
}
