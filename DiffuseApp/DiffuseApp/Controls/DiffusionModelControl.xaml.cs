using Diffuse.Common;
using Diffuse.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TensorStack.Common;
using TensorStack.Python.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for DiffusionModelControl.xaml
    /// </summary>
    public partial class DiffusionModelControl : BaseControl
    {
        private ListCollectionView _deviceCollectionView;
        private ListCollectionView _modelCollectionView;
        private ListCollectionView _controlNetCollectionView;
        private ListCollectionView _extractCollectionView;
        private ListCollectionView _loraCollectionView;
        private ListCollectionView _upscaleCollectionView;

        private ProcessType _processType;
        private DeviceModel _selectedDevice;
        private DiffusionModel _selectedModel;
        private ControlNetModel _selectedControlNet;
        private ExtractModel _selectedExtractor;
        private UpscaleModel _selectedUpscaler;
        private MemoryProfileModel _selectedMemoryMode;
        private DataType _selectedDataType;

        private bool _isControlNetSupported;
        private bool _isUpscalerSupported;
        private bool _isUpscalerEnabled;
        private bool _isLoraSupported;
        private bool _isLoraEnabled;
        private bool _isExtractorSupported;
        private bool _isExtractorEnabled;

        private DeviceModel _currentDevice;
        private DiffusionModel _currentModel;
        private ControlNetModel _currentControlNet;
        private ExtractModel _currentExtractor;
        private LoraAdapterModel[] _currentLora;
        private UpscaleModel _currentUpscaler;
        private MemoryMode _currentMemoryMode;
        private DataType _currentDataType;

        private bool _currentUpscalerEnabled;
        private bool _currentLoraEnabled;
        private bool _currentExtractorEnabled;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiffusionModelControl"/> class.
        /// </summary>
        public DiffusionModelControl()
        {
            MemoryModes =
            [
                new MemoryProfileModel{ MemoryMode = MemoryMode.Auto },
                new MemoryProfileModel{ MemoryMode = MemoryMode.Balanced },
                new MemoryProfileModel{ MemoryMode = MemoryMode.Lowest },
                new MemoryProfileModel{ MemoryMode = MemoryMode.Low },
                new MemoryProfileModel{ MemoryMode = MemoryMode.Medium },
                new MemoryProfileModel{ MemoryMode = MemoryMode.High },
                new MemoryProfileModel{ MemoryMode = MemoryMode.Highest }
            ];
            DataTypes = new ObservableCollection<DataType>();
            LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
            UnloadCommand = new AsyncRelayCommand(UnloadAsync, CanUnload);
            LoraAdapters = new ObservableCollection<LoraAdapterModel>();
            LoraAdapters.CollectionChanged += (s, e) => ValidateSelection();
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(nameof(Settings), typeof(Settings), typeof(DiffusionModelControl), new PropertyMetadata<DiffusionModelControl>((c) => c.OnSettingsChanged()));
        public static readonly DependencyProperty IsPipelineLoadedProperty = DependencyProperty.Register(nameof(IsPipelineLoaded), typeof(bool), typeof(DiffusionModelControl), new PropertyMetadata<DiffusionModelControl>((c) => c.OnIsPipelineLoadedChanged()));
        public static readonly DependencyProperty IsSelectionValidProperty = DependencyProperty.Register(nameof(IsSelectionValid), typeof(bool), typeof(DiffusionModelControl));

        public event EventHandler<PipelineModel> SelectionChanged;
        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand UnloadCommand { get; }
        public MemoryProfileModel[] MemoryModes { get; }
        public ObservableCollection<DataType> DataTypes { get; }
        public ObservableCollection<LoraAdapterModel> LoraAdapters { get; set; }

        public Settings Settings
        {
            get { return (Settings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public bool IsPipelineLoaded
        {
            get { return (bool)GetValue(IsPipelineLoadedProperty); }
            set { SetValue(IsPipelineLoadedProperty, value); }
        }

        public bool IsSelectionValid
        {
            get { return (bool)GetValue(IsSelectionValidProperty); }
            set { SetValue(IsSelectionValidProperty, value); }
        }

        public ProcessType ProcessType
        {
            get { return _processType; }
            set { SetProperty(ref _processType, value); }
        }

        public DeviceModel SelectedDevice
        {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); ValidateSelection(); }
        }

        public DiffusionModel SelectedModel
        {
            get { return _selectedModel; }
            set { SetProperty(ref _selectedModel, value); ValidateSelection(); }
        }

        public ControlNetModel SelectedControlNet
        {
            get { return _selectedControlNet; }
            set { SetProperty(ref _selectedControlNet, value); ValidateSelection(); }
        }

        public ExtractModel SelectedExtractor
        {
            get { return _selectedExtractor; }
            set { SetProperty(ref _selectedExtractor, value); ValidateSelection(); }
        }

        public UpscaleModel SelectedUpscaler
        {
            get { return _selectedUpscaler; }
            set { SetProperty(ref _selectedUpscaler, value); ValidateSelection(); }
        }

        public MemoryProfileModel SelectedMemoryMode
        {
            get { return _selectedMemoryMode; }
            set { SetProperty(ref _selectedMemoryMode, value); ValidateSelection(); }
        }

        public DataType SelectedDataType
        {
            get { return _selectedDataType; }
            set { SetProperty(ref _selectedDataType, value); ValidateSelection(); }
        }

        public ListCollectionView DeviceCollectionView
        {
            get { return _deviceCollectionView; }
            set { SetProperty(ref _deviceCollectionView, value); }
        }

        public ListCollectionView ModelCollectionView
        {
            get { return _modelCollectionView; }
            set { SetProperty(ref _modelCollectionView, value); }
        }

        public ListCollectionView ControlNetCollectionView
        {
            get { return _controlNetCollectionView; }
            set { SetProperty(ref _controlNetCollectionView, value); }
        }

        public ListCollectionView ExtractCollectionView
        {
            get { return _extractCollectionView; }
            set { SetProperty(ref _extractCollectionView, value); }
        }

        public ListCollectionView LoraCollectionView
        {
            get { return _loraCollectionView; }
            set { SetProperty(ref _loraCollectionView, value); }
        }

        public ListCollectionView UpscaleCollectionView
        {
            get { return _upscaleCollectionView; }
            set { SetProperty(ref _upscaleCollectionView, value); }
        }

        public bool IsControlNetSupported
        {
            get { return _isControlNetSupported; }
            set { SetProperty(ref _isControlNetSupported, value); }
        }

        public bool IsExtractorSupported
        {
            get { return _isExtractorSupported; }
            set { SetProperty(ref _isExtractorSupported, value); }
        }

        public bool IsExtractorEnabled
        {
            get { return _isExtractorEnabled; }
            set { SetProperty(ref _isExtractorEnabled, value); ValidateSelection(); }
        }

        public bool IsUpscalerSupported
        {
            get { return _isUpscalerSupported; }
            set { SetProperty(ref _isUpscalerSupported, value); }
        }

        public bool IsUpscalerEnabled
        {
            get { return _isUpscalerEnabled; }
            set { SetProperty(ref _isUpscalerEnabled, value); ValidateSelection(); }
        }

        public bool IsLoraSupported
        {
            get { return _isLoraSupported; }
            set { SetProperty(ref _isLoraSupported, value); }
        }

        public bool IsLoraEnabled
        {
            get { return _isLoraEnabled; }
            set { SetProperty(ref _isLoraEnabled, value); SetDefaultLora(); ValidateSelection(); }
        }


        private async Task LoadAsync()
        {
            if (!await IsAccessGrantedAsync(SelectedModel))
                return;

            _currentDevice = SelectedDevice;
            _currentModel = SelectedModel;
            _currentControlNet = SelectedControlNet;
            _currentExtractor = SelectedExtractor;
            _currentLora = _isLoraEnabled ? [.. LoraAdapters] : default;
            _currentUpscaler = SelectedUpscaler;
            _currentMemoryMode = SelectedMemoryMode.MemoryMode;
            _currentDataType = SelectedDataType;

            _currentExtractorEnabled = _isExtractorEnabled;
            _currentUpscalerEnabled = _isUpscalerEnabled;
            _currentLoraEnabled = _isLoraEnabled;

            var pipeline = new PipelineModel
            {
                Device = _currentDevice,
                DiffusionModel = _currentModel,
                ControlNetModel = _isControlNetSupported ? _currentControlNet : default,
                ExtractModel = _currentExtractorEnabled ? _currentExtractor : default,
                UpscaleModel = _currentUpscalerEnabled ? _currentUpscaler : default,
                LoraAdapterModel = _currentLoraEnabled ? _currentLora : default,
                MemoryMode = _currentMemoryMode,
                DataType = _currentDataType,
                ProcessType = _processType
            };

            SelectionChanged?.Invoke(this, pipeline);
            ValidateSelection();
        }


        private bool CanLoad()
        {
            return !IsSelectionValid;
        }


        private Task UnloadAsync()
        {
            _currentModel = default;

            IsSelectionValid = false;
            LoraAdapters.Clear();

            IsExtractorEnabled = false;
            IsLoraEnabled = false;
            IsUpscalerEnabled = false;

            _currentControlNet = default;
            _currentExtractor = default;
            _currentLora = default;
            _currentUpscaler = default;

            _currentExtractorEnabled = false;
            _currentLoraEnabled = false;
            _currentUpscalerEnabled = false;

            var pipeline = new PipelineModel
            {
                Device = _selectedDevice,
                MemoryMode = _selectedMemoryMode.MemoryMode,
                DataType = _selectedDataType,
                ProcessType = _processType
            };

            SelectionChanged?.Invoke(this, pipeline);
            Model_SelectionChanged(default, default);

            ValidateSelection();
            return Task.CompletedTask;
        }


        private bool CanUnload()
        {
            return _currentModel is not null
                || _currentControlNet is not null
                || _currentExtractor is not null
                || _currentLora is not null
                || _currentUpscaler is not null;
        }


        private Task OnIsPipelineLoadedChanged()
        {
            ValidateSelection();
            return Task.CompletedTask;
        }


        private void ValidateSelection()
        {
            var isLoraValid = !IsLoraEnabled || LoraCollectionView?.IsEmpty == false;
            var isExtractValid = !IsExtractorEnabled || ExtractCollectionView?.IsEmpty == false;
            var isUpscaleValid = !IsUpscalerEnabled || UpscaleCollectionView?.IsEmpty == false;
            var isControlNetValid = !IsControlNetSupported || ControlNetCollectionView?.IsEmpty == false;
            var isModelValid = ModelCollectionView?.IsEmpty == false;
            var isCurrentValid = !HasCurrentChanged();
            IsSelectionValid = isCurrentValid && isLoraValid && isExtractValid && isUpscaleValid && isControlNetValid && isModelValid && IsPipelineLoaded;
            LoadCommand.RaiseCanExecuteChanged();
        }


        private bool HasCurrentChanged()
        {
            return _currentDevice != SelectedDevice
                || _currentModel != SelectedModel
                || _currentControlNet != SelectedControlNet
                || _currentExtractor != SelectedExtractor
                || _currentExtractorEnabled != _isExtractorEnabled
                || _currentLoraEnabled != _isLoraEnabled
                || HasLoraChanged()
                || _currentUpscaler != SelectedUpscaler
                || _currentUpscalerEnabled != _isUpscalerEnabled
                || _currentMemoryMode != SelectedMemoryMode.MemoryMode
                || _currentDataType != SelectedDataType;
        }


        private Task OnSettingsChanged()
        {
            // Devices
            DeviceCollectionView = new ListCollectionView(Settings.Devices);
            DeviceCollectionView.Filter = (obj) =>
            {
                if (obj is not DeviceModel device)
                    return false;

                return true;
            };

            // Base Models
            ModelCollectionView = new ListCollectionView(Settings.DiffusionModels);
            ModelCollectionView.Filter = (obj) =>
            {
                if (obj is not DiffusionModel model)
                    return false;

                if (_selectedDevice is null)
                    return false;

                if (!model.ProcessTypes.Contains(_processType))
                    return false;

                if (IsControlNetSupported && !Settings.ControlNetModels.Any(x => x.Pipeline.Equals(model.Pipeline)))
                    return false;

                return true;
            };

            // ControlNet Models
            ControlNetCollectionView = new ListCollectionView(Settings.ControlNetModels);
            ControlNetCollectionView.Filter = (obj) =>
            {
                if (obj is not ControlNetModel viewModel)
                    return false;

                if (_selectedModel is null)
                    return false;

                if (_selectedModel.Pipeline != viewModel.Pipeline)
                    return false;

                return true;
            };

            // Extractor Models
            ExtractCollectionView = new ListCollectionView(Settings.ExtractModels);
            ExtractCollectionView.Filter = (obj) =>
            {
                if (obj is not ExtractModel viewModel)
                    return false;

                if (_selectedModel is null)
                    return false;

                return true;
            };

            LoraCollectionView = new ListCollectionView(Settings.LoraAdapterModels);
            LoraCollectionView.Filter = (obj) =>
            {
                if (obj is not LoraAdapterModel viewModel)
                    return false;

                if (_selectedModel is null)
                    return false;

                if (_selectedModel.Pipeline != viewModel.Pipeline)
                    return false;

                return true;
            };

            //Upscale models
            UpscaleCollectionView = new ListCollectionView(Settings.UpscaleModels);
            UpscaleCollectionView.Filter = (obj) =>
            {
                if (obj is not UpscaleModel model)
                    return false;

                if (_selectedModel is null)
                    return false;

                return true;
            };

            SelectedDevice = Settings.DefaultDevice;
            SelectedMemoryMode = MemoryModes.FirstOrDefault(x => x.MemoryMode == Settings.DefaultMemoryMode);
            return Task.CompletedTask;
        }


        private void Device_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsLoraSupported = _selectedDevice.IsLoraSupported;
            if (IsLoraEnabled && !IsLoraSupported)
                IsLoraEnabled = false;

            if (ModelCollectionView is not null)
            {
                ModelCollectionView.Refresh();
                SelectedModel = ModelCollectionView.Cast<DiffusionModel>().FirstOrDefault(x => x == _currentModel)
                             ?? ModelCollectionView.Cast<DiffusionModel>().OrderByDescending(x => x.IsDefault).FirstOrDefault();
            }

            if (ExtractCollectionView is not null)
            {
                ExtractCollectionView.Refresh();
                SelectedExtractor = ExtractCollectionView.Cast<ExtractModel>().FirstOrDefault(x => x == _currentExtractor)
                                 ?? ExtractCollectionView.Cast<ExtractModel>().OrderByDescending(x => x.IsDefault).FirstOrDefault();
            }

            if (UpscaleCollectionView is not null)
            {
                UpscaleCollectionView.Refresh();
                SelectedUpscaler = UpscaleCollectionView.Cast<UpscaleModel>().FirstOrDefault(x => x == _currentUpscaler)
                                ?? UpscaleCollectionView.Cast<UpscaleModel>().OrderByDescending(x => x.IsDefault).FirstOrDefault();
            }

            SetDeviceDataTypes();
            RefreshMemoryProfile();
        }


        private void Model_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LoraCollectionView is not null)
            {
                SetDefaultLora();
            }

            if (ControlNetCollectionView is not null)
            {
                ControlNetCollectionView.Refresh();
                SelectedControlNet = ControlNetCollectionView.Cast<ControlNetModel>().FirstOrDefault(x => x == _currentControlNet)
                                  ?? ControlNetCollectionView.Cast<ControlNetModel>().OrderByDescending(x => x.IsDefault).FirstOrDefault();
            }

            RefreshMemoryProfile();
        }


        private void SetDefaultLora()
        {
            LoraAdapters.Clear();
            LoraCollectionView.Refresh();
            var filteredLora = LoraCollectionView.Cast<LoraAdapterModel>();
            if (!_currentLora.IsNullOrEmpty() && _currentLora.Any(x => filteredLora.Contains(x)))
            {
                foreach (var lora in _currentLora)
                {
                    LoraAdapters.Add(lora);
                }
            }
            else
            {
                var defaultLora = filteredLora
                    .OrderByDescending(x => x.IsDefault)
                    .FirstOrDefault();
                if (defaultLora is not null)
                    LoraAdapters.Add(defaultLora);
            }
        }


        private void Memory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshMemoryProfile();
        }


        private void SetDeviceDataTypes()
        {
            if (_selectedDevice is null)
                return;

            DataTypes.Clear();
            foreach (var type in _selectedDevice.DataTypes)
            {
                DataTypes.Add(type);
            }

            SelectedDataType = DataTypes.Contains(Settings.DefaultDataType) ? Settings.DefaultDataType : DataTypes.Last();
        }


        private void RefreshMemoryProfile()
        {
            if (_selectedDevice is null || _selectedModel is null || _selectedMemoryMode is null)
                return;

            var profile = _selectedModel.MemoryProfile?.FirstOrDefault(x => x.DataType == _selectedDataType);
            if (profile is null)
                return;

            var deviceMemory = _selectedDevice.MemoryGB;
            var modeIndex = profile.GetIndex(deviceMemory);
            MemoryModes[0].MemoryGB = profile.MemoryModes.ElementAtOrDefault(modeIndex);
            MemoryModes[2].MemoryGB = profile.MemoryModes.ElementAtOrDefault(0);
            MemoryModes[3].MemoryGB = profile.MemoryModes.ElementAtOrDefault(1);
            MemoryModes[4].MemoryGB = profile.MemoryModes.ElementAtOrDefault(2);
            MemoryModes[5].MemoryGB = profile.MemoryModes.ElementAtOrDefault(3);
            MemoryModes[6].MemoryGB = profile.MemoryModes.ElementAtOrDefault(4);
        }


        public bool HasLoraChanged()
        {
            if (!_isLoraSupported || !_isLoraEnabled)
                return false;

            return _currentLora.HasChanged(LoraAdapters);
        }


        public void SetPipeline(PipelineModel pipeline)
        {
            if (pipeline == null)
                return;

            if (!ModelCollectionView.Contains(pipeline.DiffusionModel))
                return;

            SelectedDevice = pipeline.Device;
            SelectedModel = pipeline.DiffusionModel;

            SelectedDataType = pipeline.DataType;
            SelectedMemoryMode = SelectedMemoryMode = MemoryModes.FirstOrDefault(x => x.MemoryMode == pipeline.MemoryMode);

            if (IsUpscalerSupported)
            {
                IsUpscalerEnabled = pipeline.UpscaleModel is not null;
                if (pipeline.UpscaleModel is not null)
                    SelectedUpscaler = pipeline.UpscaleModel;
            }

            if (IsExtractorSupported)
            {
                IsExtractorEnabled = pipeline.ExtractModel is not null;
                if (pipeline.ExtractModel is not null)
                    SelectedExtractor = pipeline.ExtractModel;
            }

            if (IsControlNetSupported)
            {
                if (pipeline.ControlNetModel is not null)
                    SelectedControlNet = pipeline.ControlNetModel;
            }

            if (IsLoraSupported)
            {
                IsLoraEnabled = !pipeline.LoraAdapterModel.IsNullOrEmpty();
                if (IsLoraEnabled)
                {
                    LoraAdapters.Clear();
                    foreach (var loraAdapter in pipeline.LoraAdapterModel)
                    {
                        LoraAdapters.Add(loraAdapter);
                    }
                }
            }

            ValidateSelection();
        }



        private async Task<bool> IsAccessGrantedAsync(DiffusionModel model)
        {
            if (!model.IsGated)
                return true;

            if (!string.IsNullOrEmpty(Settings.SecureToken))
                return true;

            var dialog = DialogService.GetDialog<GatedModelDialog>();
            await dialog.ShowDialogAsync(model);
            return false;
        }

    }
}
