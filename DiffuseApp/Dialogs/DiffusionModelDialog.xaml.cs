// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Python.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for DiffusionModelDialog.xaml
    /// </summary>
    public partial class DiffusionModelDialog : DialogControl
    {
        private bool _isCustomPipeline;
        private SizeOption _selectedSize;
        private DiffusionModel _diffusionModel;
        private SchedulerType _selectedScheduler;
        private DiffusionModel _originalDiffusionModel;

        public DiffusionModelDialog(Settings settings)
        {
            Settings = settings;
            Sizes = new ObservableCollection<SizeOption>();
            Schedulers = new ObservableCollection<SchedulerType>();
            Pipelines = new ObservableCollection<string>(GetPipelines());
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            AddSizeCommand = new AsyncRelayCommand(AddSizeAsync, CanAddSize);
            RemoveSizeCommand = new AsyncRelayCommand<SizeOption>(RemoveSizeAsync);
            AddSchedulerCommand = new AsyncRelayCommand(AddSchedulerAsync, CanAddScheduler);
            RemoveSchedulerCommand = new AsyncRelayCommand<SchedulerType>(RemoveSchedulerAsync);
            InitializeComponent();
        }

        public Settings Settings { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand AddSizeCommand { get; }
        public AsyncRelayCommand<SizeOption> RemoveSizeCommand { get; }
        public AsyncRelayCommand AddSchedulerCommand { get; }
        public AsyncRelayCommand<SchedulerType> RemoveSchedulerCommand { get; }
        public ObservableCollection<SizeOption> Sizes { get; }
        public ObservableCollection<SchedulerType> Schedulers { get; }
        public ObservableCollection<string> Pipelines { get; }
        public bool IsUpdateMode => _originalDiffusionModel is not null;

        public DiffusionModel DiffusionModel
        {
            get { return _diffusionModel; }
            set { SetProperty(ref _diffusionModel, value); }
        }

        public SizeOption SelectedSize
        {
            get { return _selectedSize; }
            set { SetProperty(ref _selectedSize, value); }
        }

        public SchedulerType SelectedScheduler
        {
            get { return _selectedScheduler; }
            set { SetProperty(ref _selectedScheduler, value); }
        }

        public bool IsCustomPipeline
        {
            get { return _isCustomPipeline; }
            set
            {
                SetProperty(ref _isCustomPipeline, value);
                if (!_isCustomPipeline)
                {
                    DiffusionModel.Pipeline = Pipelines.FirstOrDefault(x => x == DiffusionModel.Pipeline) ?? Pipelines.FirstOrDefault();
                    DiffusionModel.NotifyPropertyChanged(nameof(DiffusionModel.Pipeline));
                }
            }
        }


        public Task<bool> AddAsync()
        {
            var modelId = GetNextModelId();
            DiffusionModel = new DiffusionModel
            {
                Id = modelId,
                MemoryModes = [0, 0, 0, 0],
                DefaultOptions = new DiffusionDefaultOptions { },
            };
            SelectedSize = new SizeOption
            {
                Height = 512,
                Width = 512,
                IsDefault = true
            };
            return base.ShowDialogAsync();
        }


        public Task<bool> UpdateAsync(DiffusionModel diffusionModel)
        {
            var modelId = diffusionModel.Id;
            _originalDiffusionModel = diffusionModel;
            DiffusionModel = DeepClone(diffusionModel, modelId);
            Populate();
            return base.ShowDialogAsync();
        }


        public Task<bool> CopyAsync(DiffusionModel diffusionModel)
        {
            var modelId = GetNextModelId();
            DiffusionModel = DeepClone(diffusionModel, modelId);
            Populate();
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            var index = Settings.DiffusionModels.Count;
            if (IsUpdateMode)
            {
                index = Settings.DiffusionModels.IndexOf(_originalDiffusionModel);
                Settings.DiffusionModels.Remove(_originalDiffusionModel);
            }

            DiffusionModel.DataTypes = GetDataTypes();
            DiffusionModel.Resolutions = [.. Sizes];
            DiffusionModel.ProcessTypes = GetProcessTypes();

            var defaultSize = Sizes.FirstOrDefault(x => x.IsDefault);
            DiffusionModel.DefaultOptions.Width = defaultSize.Width;
            DiffusionModel.DefaultOptions.Height = defaultSize.Height;

            Settings.DiffusionModels.Insert(index, DiffusionModel);
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(DiffusionModel?.Name))
                return false;
            if (string.IsNullOrWhiteSpace(DiffusionModel?.ModelUrl))
                return false;
            if (string.IsNullOrWhiteSpace(DiffusionModel?.Pipeline))
                return false;

            return base.CanExecuteCancel();
        }


        protected override Task CancelAsync()
        {
            DiffusionModel = default;
            _originalDiffusionModel = null;
            return base.CancelAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private Task AddSizeAsync()
        {
            if (!CanAddSize())
                return Task.CompletedTask;

            if (SelectedSize.IsDefault)
            {
                foreach (var size in Sizes)
                    size.IsDefault = false;
            }

            Sizes.Add(new SizeOption
            {
                Width = SelectedSize.Width,
                Height = SelectedSize.Height,
                IsDefault = SelectedSize.IsDefault,
            });

            SelectedSize.IsDefault = false;
            NotifyPropertyChanged(nameof(SelectedSize));
            return Task.CompletedTask;
        }


        private bool CanAddSize()
        {
            return SelectedSize.Width > 0
                && SelectedSize.Height > 0
                && !Sizes.Any(x => x.Width == SelectedSize.Width && x.Height == SelectedSize.Height);
        }


        private Task RemoveSizeAsync(SizeOption sizeOption)
        {
            Sizes.Remove(sizeOption);
            return Task.CompletedTask;
        }


        private Task AddSchedulerAsync()
        {
            if (!CanAddScheduler())
                return Task.CompletedTask;

            Schedulers.Add(SelectedScheduler);
            return Task.CompletedTask;
        }


        private bool CanAddScheduler()
        {
            return !Schedulers.Any(x => x == SelectedScheduler);
        }


        private Task RemoveSchedulerAsync(SchedulerType type)
        {
            Schedulers.Remove(type);
            return Task.CompletedTask;
        }


        private void Populate()
        {
            foreach (var size in DiffusionModel.Resolutions)
                Sizes.Add(size);

            foreach (var scheduler in DiffusionModel.DefaultOptions.Schedulers)
                Schedulers.Add(scheduler);

            SetDataTypes();
            SetProcessTypes();
            SelectedScheduler = DiffusionModel.DefaultOptions.Scheduler;
            SelectedSize = Sizes.FirstOrDefault(x => x.IsDefault) ?? Sizes.FirstOrDefault();
            NotifyPropertyChanged(nameof(IsUpdateMode));
        }


        private int GetNextModelId()
        {
            return Settings.DiffusionModels.Max(x => x.Id) + 1;
        }


        private void SetProcessTypes()
        {
            foreach (var processType in DiffusionModel.ProcessTypes)
            {
                if (processType == ProcessType.TextToImage)
                    CheckBoxTextToImage.IsChecked = true;
                if (processType == ProcessType.ImageToImage)
                    CheckBoxImageToImage.IsChecked = true;
                if (processType == ProcessType.ImageEdit)
                    CheckBoxImageEdit.IsChecked = true;
                if (processType == ProcessType.ControlNetImage)
                    CheckBoxControlNetImage.IsChecked = true;
                if (processType == ProcessType.ControlNetImageToImage)
                    CheckBoxControlNetImageToImage.IsChecked = true;
                if (processType == ProcessType.TextToVideo)
                    CheckBoxTextToVideo.IsChecked = true;
                if (processType == ProcessType.ImageToVideo)
                    CheckBoxImageToVideo.IsChecked = true;
                if (processType == ProcessType.VideoToVideo)
                    CheckBoxVideoToVideo.IsChecked = true;
            }
        }


        private ProcessType[] GetProcessTypes()
        {
            IEnumerable<ProcessType> ProcessTypes()
            {
                if (CheckBoxTextToImage.IsChecked == true)
                    yield return ProcessType.TextToImage;
                if (CheckBoxImageToImage.IsChecked == true)
                    yield return ProcessType.ImageToImage;
                if (CheckBoxImageEdit.IsChecked == true)
                    yield return ProcessType.ImageEdit;
                if (CheckBoxControlNetImage.IsChecked == true)
                    yield return ProcessType.ControlNetImage;
                if (CheckBoxControlNetImageToImage.IsChecked == true)
                    yield return ProcessType.ControlNetImageToImage;
                if (CheckBoxTextToVideo.IsChecked == true)
                    yield return ProcessType.TextToVideo;
                if (CheckBoxImageToVideo.IsChecked == true)
                    yield return ProcessType.ImageToVideo;
                if (CheckBoxVideoToVideo.IsChecked == true)
                    yield return ProcessType.VideoToVideo;
            }
            return [.. ProcessTypes()];
        }


        private void SetDataTypes()
        {
            foreach (var dataTypes in DiffusionModel.DataTypes)
            {
                if (dataTypes == DataType.Float32)
                    CheckBoxDataTypeFloat32.IsChecked = true;
                if (dataTypes == DataType.Bfloat16)
                    CheckBoxDataTypeBFloat16.IsChecked = true;
                if (dataTypes == DataType.Float16)
                    CheckBoxDataTypeFloat16.IsChecked = true;
                if (dataTypes == DataType.Float8_e4m3fn)
                    CheckBoxDataTypeFloat8E4.IsChecked = true;
                if (dataTypes == DataType.Float8_e5m2)
                    CheckBoxDataTypeFloat8E5.IsChecked = true;
            }
        }


        private DataType[] GetDataTypes()
        {
            IEnumerable<DataType> DataTypes()
            {
                if (CheckBoxDataTypeFloat32.IsChecked == true)
                    yield return DataType.Float32;
                if (CheckBoxDataTypeBFloat16.IsChecked == true)
                    yield return DataType.Bfloat16;
                if (CheckBoxDataTypeFloat16.IsChecked == true)
                    yield return DataType.Float16;
                if (CheckBoxDataTypeFloat8E4.IsChecked == true)
                    yield return DataType.Float8_e4m3fn;
                if (CheckBoxDataTypeFloat8E5.IsChecked == true)
                    yield return DataType.Float8_e5m2;
            }
            return [.. DataTypes()];
        }


        private IEnumerable<string> GetPipelines()
        {
            var pipelines = new List<string>(
            [
                "ChromaPipeline", 
                "FluxPipeline",
                "Flux2Pipeline", 
                "Kandinsky5Pipeline",
                "QwenImagePipeline", 
                "StableDiffusionXLPipeline",
                "WanPipeline", 
                "ZImagePipeline"
            ]);
            pipelines.AddRange(Settings.DiffusionModels.Select(x => x.Pipeline));
            return pipelines.Distinct();
        }


        private static DiffusionModel DeepClone(DiffusionModel diffusionModel, int modelId)
        {
            return new DiffusionModel
            {
                Id = modelId,
                Name = diffusionModel.Name,
                ModelUrl = diffusionModel.ModelUrl,
                Pipeline = diffusionModel.Pipeline,
                IsDefault = diffusionModel.IsDefault,
                MemoryModes = [.. diffusionModel.MemoryModes],
                DataTypes = [.. diffusionModel.DataTypes],
                ProcessTypes = [.. diffusionModel.ProcessTypes],
                Resolutions = [.. diffusionModel.Resolutions.Select(x => new SizeOption
                {
                    Height = x.Height,
                    Width = x.Width,
                    IsDefault = x.IsDefault,
                })],
                DefaultOptions = new DiffusionDefaultOptions
                {
                    Width = diffusionModel.DefaultOptions.Width,
                    Height = diffusionModel.DefaultOptions.Height,
                    Frames = diffusionModel.DefaultOptions.Frames,
                    GuidanceScale = diffusionModel.DefaultOptions.GuidanceScale,
                    GuidanceScale2 = diffusionModel.DefaultOptions.GuidanceScale2,
                    FrameRate = diffusionModel.DefaultOptions.FrameRate,
                    Scheduler = diffusionModel.DefaultOptions.Scheduler,
                    Schedulers = [.. diffusionModel.DefaultOptions.Schedulers],
                    Shift = diffusionModel.DefaultOptions.Shift,
                    Steps = diffusionModel.DefaultOptions.Steps,
                    Steps2 = diffusionModel.DefaultOptions.Steps2
                }
            };
        }
    }
}
