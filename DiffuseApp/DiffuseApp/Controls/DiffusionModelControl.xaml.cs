using Diffuse.Common;
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
        private Device _selectedDevice;
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

        private Device _currentDevice;
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
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(nameof(Settings), typeof(Settings), typeof(DiffusionModelControl), new PropertyMetadata<DiffusionModelControl>((c) => c.OnSettingsChanged()));
        public static readonly DependencyProperty CurrentPipelineProperty = DependencyProperty.Register(nameof(CurrentPipeline), typeof(PipelineModel), typeof(DiffusionModelControl), new PropertyMetadata<DiffusionModelControl>((c) => c.OnCurrentPipelineChanged()));
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

        public PipelineModel CurrentPipeline
        {
            get { return (PipelineModel)GetValue(CurrentPipelineProperty); }
            set { SetValue(CurrentPipelineProperty, value); }
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

        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); }
        }

        public DiffusionModel SelectedModel
        {
            get { return _selectedModel; }
            set { SetProperty(ref _selectedModel, value); }
        }

        public ControlNetModel SelectedControlNet
        {
            get { return _selectedControlNet; }
            set { SetProperty(ref _selectedControlNet, value); }
        }

        public ExtractModel SelectedExtractor
        {
            get { return _selectedExtractor; }
            set { SetProperty(ref _selectedExtractor, value); }
        }

        public UpscaleModel SelectedUpscaler
        {
            get { return _selectedUpscaler; }
            set { SetProperty(ref _selectedUpscaler, value); }
        }

        public MemoryProfileModel SelectedMemoryMode
        {
            get { return _selectedMemoryMode; }
            set { SetProperty(ref _selectedMemoryMode, value); }
        }

        public DataType SelectedDataType
        {
            get { return _selectedDataType; }
            set { SetProperty(ref _selectedDataType, value); }
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
            set { SetProperty(ref _isExtractorEnabled, value); }
        }

        public bool IsUpscalerSupported
        {
            get { return _isUpscalerSupported; }
            set { SetProperty(ref _isUpscalerSupported, value); }
        }

        public bool IsUpscalerEnabled
        {
            get { return _isUpscalerEnabled; }
            set { SetProperty(ref _isUpscalerEnabled, value); }
        }

        public bool IsLoraSupported
        {
            get { return _isLoraSupported; }
            set { SetProperty(ref _isLoraSupported, value); }
        }

        public bool IsLoraEnabled
        {
            get { return _isLoraEnabled; }
            set { SetProperty(ref _isLoraEnabled, value); }
        }


        private Task LoadAsync()
        {
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

            CurrentPipeline = new PipelineModel
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

            SelectionChanged?.Invoke(this, CurrentPipeline);
            return Task.CompletedTask;
        }


        private bool CanLoad()
        {
            var isReloadRequired = SelectedDevice is not null
                && SelectedModel is not null
                && (!IsControlNetSupported || SelectedControlNet is not null)
                && (!IsExtractorEnabled || SelectedExtractor is not null)
                && (!IsLoraEnabled || IsLoraValid())
                && (!IsUpscalerEnabled || SelectedUpscaler is not null)
                && HasCurrentChanged();

            var isSelectionValid = !isReloadRequired;
            if (IsSelectionValid != isSelectionValid)
                IsSelectionValid = isSelectionValid;

            return isReloadRequired;
        }


        private Task UnloadAsync()
        {
            _currentModel = default;

            SelectedControlNet = default;
            SelectedExtractor = default;
            LoraAdapters.Clear();
            SelectedUpscaler = default;

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

            CurrentPipeline = new PipelineModel
            {
                Device = _selectedDevice,
                MemoryMode = _selectedMemoryMode.MemoryMode,
                DataType = _selectedDataType,
            };

            SelectionChanged?.Invoke(this, CurrentPipeline);
            Model_SelectionChanged(default, default);
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


        private bool HasCurrentChanged()
        {
            return _currentDevice != SelectedDevice
                || _currentModel != SelectedModel
                || _currentControlNet != SelectedControlNet
                || _currentExtractor != SelectedExtractor
                || _currentExtractorEnabled != _isExtractorEnabled
                || HasLoraChanged()
                || _currentLoraEnabled != _isLoraEnabled
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
                if (obj is not Device device)
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


        private Task OnCurrentPipelineChanged()
        {
            if (CurrentPipeline is null)
            {
                _currentModel = default;
            }
            return Task.CompletedTask;
        }


        private void Device_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
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

            if (ControlNetCollectionView is not null)
            {
                ControlNetCollectionView.Refresh();
                SelectedControlNet = ControlNetCollectionView.Cast<ControlNetModel>().FirstOrDefault(x => x == _currentControlNet)
                                  ?? ControlNetCollectionView.Cast<ControlNetModel>().OrderByDescending(x => x.IsDefault).FirstOrDefault();
            }

            RefreshMemoryProfile();
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
            DataTypes.Add(DataType.Int8);
            DataTypes.Add(DataType.Float16);
            DataTypes.Add(DataType.Bfloat16);
            SelectedDataType = DataTypes.Contains(Settings.DefaultDataType) ? Settings.DefaultDataType : DataTypes.First();
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




        private bool IsLoraValid()
        {
            return LoraAdapters.Count > 0 && LoraAdapters.All(x => !string.IsNullOrEmpty(x.Name));
        }

        public bool HasLoraChanged()
        {
            if (_currentLora == null && LoraAdapters == null)
                return false;
            if (_currentLora == null || LoraAdapters == null)
                return true;
            if (_currentLora.Length != LoraAdapters.Count)
                return true;
            for (int i = 0; i < _currentLora.Length; i++)
            {
                if (!string.Equals(_currentLora[i]?.Name, LoraAdapters[i]?.Name, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }

    public class MemoryProfileModel : BaseModel
    {
        private int _memoryGB;
        private MemoryMode _memoryMode;

        public MemoryMode MemoryMode
        {
            get { return _memoryMode; }
            set { SetProperty(ref _memoryMode, value); }
        }
        public int MemoryGB
        {
            get { return _memoryGB; }
            set { SetProperty(ref _memoryGB, value); }
        }
    }


}
