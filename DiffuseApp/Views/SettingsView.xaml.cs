using Diffuse.Common;
using Diffuse.Dialogs;
using Diffuse.Services;
using System;
using System.Threading.Tasks;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : ViewBase
    {
        private DiffusionModel _selectedDiffusionModel;
        private ControlNetModel _selectedControlNetModel;
        private ExtractorModel _selectedExtractorModel;
        private LoraAdapterModel _selectedLoraModel;
        private UpscaleModel _selectedUpscaleModel;

        public SettingsView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(settings, navigationService, environmentService, historyService)
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            AddDiffusionModelCommand = new AsyncRelayCommand(AddDiffusionModelAsync);
            CopyDiffusionModelCommand = new AsyncRelayCommand(CopyDiffusionModelAsync, () => SelectedDiffusionModel is not null);
            UpdateDiffusionModelCommand = new AsyncRelayCommand(UpdateDiffusionModelAsync, () => SelectedDiffusionModel is not null);
            RemoveDiffusionModelCommand = new AsyncRelayCommand(RemoveDiffusionModelAsync, () => SelectedDiffusionModel is not null);
            AddControlNetModelCommand = new AsyncRelayCommand(AddControlNetModelAsync);
            CopyControlNetModelCommand = new AsyncRelayCommand(CopyControlNetModelAsync, () => SelectedControlNetModel is not null);
            UpdateControlNetModelCommand = new AsyncRelayCommand(UpdateControlNetModelAsync, () => SelectedControlNetModel is not null);
            RemoveControlNetModelCommand = new AsyncRelayCommand(RemoveControlNetModelAsync, () => SelectedControlNetModel is not null);
            AddExtractorModelCommand = new AsyncRelayCommand(AddExtractorModelAsync);
            CopyExtractorModelCommand = new AsyncRelayCommand(CopyExtractorModelAsync, () => SelectedExtractorModel is not null);
            UpdateExtractorModelCommand = new AsyncRelayCommand(UpdateExtractorModelAsync, () => SelectedExtractorModel is not null);
            RemoveExtractorModelCommand = new AsyncRelayCommand(RemoveExtractorModelAsync, () => SelectedExtractorModel is not null);
            AddLoraModelCommand = new AsyncRelayCommand(AddLoraModelAsync);
            CopyLoraModelCommand = new AsyncRelayCommand(CopyLoraModelAsync, () => SelectedLoraModel is not null);
            UpdateLoraModelCommand = new AsyncRelayCommand(UpdateLoraModelAsync, () => SelectedLoraModel is not null);
            RemoveLoraModelCommand = new AsyncRelayCommand(RemoveLoraModelAsync, () => SelectedLoraModel is not null);
            AddUpscaleModelCommand = new AsyncRelayCommand(AddUpscaleModel);
            CopyUpscaleModelCommand = new AsyncRelayCommand(CopyUpscaleModelAsync, () => SelectedUpscaleModel is not null);
            UpdateUpscaleModelCommand = new AsyncRelayCommand(UpdateUpscaleModelAsync, () => SelectedUpscaleModel is not null);
            RemoveUpscaleModelCommand = new AsyncRelayCommand(RemoveUpscaleModelAsync, () => SelectedUpscaleModel is not null);
            InitializeComponent();
        }

        public override int Id => (int)View.Settings; 
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand AddDiffusionModelCommand { get; }
        public AsyncRelayCommand CopyDiffusionModelCommand { get; }
        public AsyncRelayCommand UpdateDiffusionModelCommand { get; }
        public AsyncRelayCommand RemoveDiffusionModelCommand { get; }
        public AsyncRelayCommand AddControlNetModelCommand { get; }
        public AsyncRelayCommand CopyControlNetModelCommand { get; }
        public AsyncRelayCommand UpdateControlNetModelCommand { get; }
        public AsyncRelayCommand RemoveControlNetModelCommand { get; }
        public AsyncRelayCommand AddExtractorModelCommand { get; }
        public AsyncRelayCommand CopyExtractorModelCommand { get; }
        public AsyncRelayCommand UpdateExtractorModelCommand { get; }
        public AsyncRelayCommand RemoveExtractorModelCommand { get; }
        public AsyncRelayCommand AddLoraModelCommand { get; }
        public AsyncRelayCommand CopyLoraModelCommand { get; }
        public AsyncRelayCommand UpdateLoraModelCommand { get; }
        public AsyncRelayCommand RemoveLoraModelCommand { get; }
        public AsyncRelayCommand AddUpscaleModelCommand { get; }
        public AsyncRelayCommand UpdateUpscaleModelCommand { get; }
        public AsyncRelayCommand CopyUpscaleModelCommand { get; }
        public AsyncRelayCommand RemoveUpscaleModelCommand { get; }

        public DiffusionModel SelectedDiffusionModel
        {
            get { return _selectedDiffusionModel; }
            set { SetProperty(ref _selectedDiffusionModel, value); }
        }

        public ControlNetModel SelectedControlNetModel
        {
            get { return _selectedControlNetModel; }
            set { SetProperty(ref _selectedControlNetModel, value); }
        }

        public ExtractorModel SelectedExtractorModel
        {
            get { return _selectedExtractorModel; }
            set { SetProperty(ref _selectedExtractorModel, value); }
        }

        public LoraAdapterModel SelectedLoraModel
        {
            get { return _selectedLoraModel; }
            set { SetProperty(ref _selectedLoraModel, value); }
        }

        public UpscaleModel SelectedUpscaleModel
        {
            get { return _selectedUpscaleModel; }
            set { SetProperty(ref _selectedUpscaleModel, value); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            return base.OpenAsync(args);
        }


        private async Task AddDiffusionModelAsync()
        {
            var dialog = DialogService.GetDialog<DiffusionModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }

        private async Task CopyDiffusionModelAsync()
        {
            var dialog = DialogService.GetDialog<DiffusionModelDialog>();
            if (await dialog.CopyAsync(SelectedDiffusionModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateDiffusionModelAsync()
        {
            var dialog = DialogService.GetDialog<DiffusionModelDialog>();
            if (await dialog.UpdateAsync(SelectedDiffusionModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveDiffusionModelAsync()
        {
            Settings.DiffusionModels.Remove(SelectedDiffusionModel);
            SelectedDiffusionModel = default;
            await SaveAsync();
        }


        private async Task AddControlNetModelAsync()
        {
            var dialog = DialogService.GetDialog<ControlNetModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }


        private async Task CopyControlNetModelAsync()
        {
            var dialog = DialogService.GetDialog<ControlNetModelDialog>();
            if (await dialog.CopyAsync(SelectedControlNetModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateControlNetModelAsync()
        {
            var dialog = DialogService.GetDialog<ControlNetModelDialog>();
            if (await dialog.UpdateAsync(SelectedControlNetModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveControlNetModelAsync()
        {
            Settings.ControlNetModels.Remove(SelectedControlNetModel);
            SelectedControlNetModel = default;
            await SaveAsync();
        }


        private async Task AddExtractorModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractorModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }


        private async Task CopyExtractorModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractorModelDialog>();
            if (await dialog.CopyAsync(SelectedExtractorModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateExtractorModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractorModelDialog>();
            if (await dialog.UpdateAsync(SelectedExtractorModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveExtractorModelAsync()
        {
            Settings.ExtractorModels.Remove(SelectedExtractorModel);
            SelectedExtractorModel = default;
            await SaveAsync();
        }


        private async Task AddLoraModelAsync()
        {
            var dialog = DialogService.GetDialog<LoraModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }


        private async Task CopyLoraModelAsync()
        {
            var dialog = DialogService.GetDialog<LoraModelDialog>();
            if (await dialog.CopyAsync(SelectedLoraModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateLoraModelAsync()
        {
            var dialog = DialogService.GetDialog<LoraModelDialog>();
            if (await dialog.UpdateAsync(SelectedLoraModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveLoraModelAsync()
        {
            Settings.LoraAdapterModels.Remove(SelectedLoraModel);
            SelectedLoraModel = default;
            await SaveAsync();
        }


        private async Task AddUpscaleModel()
        {
            var dialog = DialogService.GetDialog<UpscaleModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }


        private async Task CopyUpscaleModelAsync()
        {
            var dialog = DialogService.GetDialog<UpscaleModelDialog>();
            if (await dialog.UpdateAsync(SelectedUpscaleModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateUpscaleModelAsync()
        {
            var dialog = DialogService.GetDialog<UpscaleModelDialog>();
            if (await dialog.UpdateAsync(SelectedUpscaleModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveUpscaleModelAsync()
        {
            Settings.UpscaleModels.Remove(SelectedUpscaleModel);
            SelectedUpscaleModel = default;
            await SaveAsync();
        }


        private async Task SaveAsync()
        {
           await Json.SaveAsync<Settings>("Settings.json", Settings);
        }
    }
}