using Diffuse.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TensorStack.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for ModelControl.xaml
    /// </summary>
    public partial class ModelControl : BaseControl
    {
        private ListCollectionView _deviceCollectionView;
        private ListCollectionView _extractCollectionView;
        private ListCollectionView _upscaleCollectionView;

        private Device _selectedDevice;
        private ExtractModel _selectedExtractor;
        private UpscaleModel _selectedUpscaler;

        private bool _isUpscalerEnabled;
        private bool _isExtractorEnabled;

        private Device _currentDevice;
        private ExtractModel _currentExtractor;
        private UpscaleModel _currentUpscaler;


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelControl"/> class.
        /// </summary>
        public ModelControl()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
            UnloadCommand = new AsyncRelayCommand(UnloadAsync, CanUnload);
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(nameof(Settings), typeof(Settings), typeof(ModelControl), new PropertyMetadata<ModelControl>((c) => c.OnSettingsChanged()));
        public static readonly DependencyProperty IsPipelineLoadedProperty = DependencyProperty.Register(nameof(IsPipelineLoaded), typeof(bool), typeof(ModelControl), new PropertyMetadata<ModelControl>((c) => c.OnIsPipelineLoadedChanged()));
        public static readonly DependencyProperty IsSelectionValidProperty = DependencyProperty.Register(nameof(IsSelectionValid), typeof(bool), typeof(ModelControl));

        public event EventHandler<PipelineModel> SelectionChanged;
        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand UnloadCommand { get; }

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

        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); ValidateSelection(); }
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

        public ListCollectionView DeviceCollectionView
        {
            get { return _deviceCollectionView; }
            set { SetProperty(ref _deviceCollectionView, value); }
        }

        public ListCollectionView ExtractCollectionView
        {
            get { return _extractCollectionView; }
            set { SetProperty(ref _extractCollectionView, value); }
        }

        public ListCollectionView UpscaleCollectionView
        {
            get { return _upscaleCollectionView; }
            set { SetProperty(ref _upscaleCollectionView, value); }
        }

        public bool IsExtractorEnabled
        {
            get { return _isExtractorEnabled; }
            set { SetProperty(ref _isExtractorEnabled, value); }
        }

        public bool IsUpscalerEnabled
        {
            get { return _isUpscalerEnabled; }
            set { SetProperty(ref _isUpscalerEnabled, value); }
        }


        private Task LoadAsync()
        {
            _currentDevice = SelectedDevice;
            _currentExtractor = SelectedExtractor;
            _currentUpscaler = SelectedUpscaler;

            var pipeline = new PipelineModel
            {
                Device = _currentDevice,
                ExtractModel = _isExtractorEnabled ? _currentExtractor : default,
                UpscaleModel = _isUpscalerEnabled ? _currentUpscaler : default,
            };

            ValidateSelection();
            SelectionChanged?.Invoke(this, pipeline);
            return Task.CompletedTask;
        }


        private bool CanLoad()
        {
            return !IsSelectionValid;
        }


        private Task UnloadAsync()
        {
            _currentExtractor = default;
            _currentUpscaler = default;
            SelectionChanged?.Invoke(this, default);
            ValidateSelection();
            return Task.CompletedTask;
        }


        private bool CanUnload()
        {
            return _currentExtractor is not null
                || _currentUpscaler is not null;
        }


        private bool HasCurrentChanged()
        {
            return _currentDevice != SelectedDevice
                || (IsExtractorEnabled && _currentExtractor != SelectedExtractor)
                || (IsUpscalerEnabled && _currentUpscaler != SelectedUpscaler);
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


            // Extractor Models
            ExtractCollectionView = new ListCollectionView(Settings.ExtractModels);
            ExtractCollectionView.Filter = (obj) =>
            {
                if (obj is not ExtractModel viewModel)
                    return false;

                if (_selectedDevice is null)
                    return false;

                return true;
            };


            //Upscale models
            UpscaleCollectionView = new ListCollectionView(Settings.UpscaleModels);
            UpscaleCollectionView.Filter = (obj) =>
            {
                if (obj is not UpscaleModel model)
                    return false;

                if (_selectedDevice is null)
                    return false;

                return true;
            };

            SelectedDevice = Settings.DefaultDevice;
            return Task.CompletedTask;
        }


        private void Device_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ExtractCollectionView is not null)
            {
                ExtractCollectionView.Refresh();
                SelectedExtractor = ExtractCollectionView.Cast<ExtractModel>().FirstOrDefault(x => x == _currentExtractor)
                                 ?? ExtractCollectionView.Cast<ExtractModel>().FirstOrDefault(x => x.IsDefault)
                                 ?? ExtractCollectionView.Cast<ExtractModel>().FirstOrDefault();
            }

            if (UpscaleCollectionView is not null)
            {
                UpscaleCollectionView.Refresh();
                SelectedUpscaler = UpscaleCollectionView.Cast<UpscaleModel>().FirstOrDefault(x => x == _currentUpscaler)
                                ?? UpscaleCollectionView.Cast<UpscaleModel>().FirstOrDefault(x => x.IsDefault)
                                ?? UpscaleCollectionView.Cast<UpscaleModel>().FirstOrDefault();
            }
        }


        private Task OnIsPipelineLoadedChanged()
        {
            ValidateSelection();
            return Task.CompletedTask;
        }


        private void ValidateSelection()
        {
            var isExtractValid = !IsExtractorEnabled || ExtractCollectionView?.IsEmpty == false;
            var isUpscaleValid = !IsUpscalerEnabled || UpscaleCollectionView?.IsEmpty == false;
            var isCurrentValid = !HasCurrentChanged();
            IsSelectionValid = isCurrentValid && isExtractValid && isUpscaleValid && IsPipelineLoaded;
            LoadCommand.RaiseCanExecuteChanged();
        }


        public void SetPipeline(PipelineModel pipeline)
        {
            if (pipeline == null)
                return;

            SelectedDevice = pipeline.Device;
            if (IsUpscalerEnabled && pipeline.UpscaleModel is not null)
                SelectedUpscaler = pipeline.UpscaleModel;

            if (IsExtractorEnabled && pipeline.ExtractModel is not null)
                SelectedExtractor = pipeline.ExtractModel;

            ValidateSelection();
        }
    }
}
