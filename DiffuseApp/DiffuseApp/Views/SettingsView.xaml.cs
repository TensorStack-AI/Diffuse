using Diffuse.Common;
using Diffuse.Dialogs;
using Diffuse.Services;
using System;
using System.Linq;
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
        private const int _fixedIdRange = 100;
        private EnvironmentModel _selectedEnvironment;
        private DiffusionModel _selectedDiffusionModel;
        private ControlNetModel _selectedControlNetModel;
        private ExtractModel _selectedExtractModel;
        private LoraAdapterModel _selectedLoraModel;
        private UpscaleModel _selectedUpscaleModel;

        public SettingsView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(settings, navigationService, environmentService, historyService)
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            AddEnvironmentCommand = new AsyncRelayCommand(AddEnvironmentAsync);
            CopyEnvironmentCommand = new AsyncRelayCommand(CopyEnvironmentAsync, () => SelectedEnvironment is not null);
            UpdateEnvironmentCommand = new AsyncRelayCommand(UpdateEnvironmentAsync, () => SelectedEnvironment?.Id > _fixedIdRange);
            RemoveEnvironmentCommand = new AsyncRelayCommand(RemoveEnvironmentAsync, () => SelectedEnvironment?.Id > _fixedIdRange);

            AddDiffusionModelCommand = new AsyncRelayCommand(AddDiffusionModelAsync);
            CopyDiffusionModelCommand = new AsyncRelayCommand(CopyDiffusionModelAsync, () => SelectedDiffusionModel is not null);
            UpdateDiffusionModelCommand = new AsyncRelayCommand(UpdateDiffusionModelAsync, () => SelectedDiffusionModel?.Id > _fixedIdRange);
            RemoveDiffusionModelCommand = new AsyncRelayCommand(RemoveDiffusionModelAsync, () => SelectedDiffusionModel?.Id > _fixedIdRange);

            AddControlNetModelCommand = new AsyncRelayCommand(AddControlNetModelAsync);
            CopyControlNetModelCommand = new AsyncRelayCommand(CopyControlNetModelAsync, () => SelectedControlNetModel is not null);
            UpdateControlNetModelCommand = new AsyncRelayCommand(UpdateControlNetModelAsync, () => SelectedControlNetModel?.Id > _fixedIdRange);
            RemoveControlNetModelCommand = new AsyncRelayCommand(RemoveControlNetModelAsync, () => SelectedControlNetModel?.Id > _fixedIdRange);

            AddExtractModelCommand = new AsyncRelayCommand(AddExtractModelAsync);
            CopyExtractModelCommand = new AsyncRelayCommand(CopyExtractModelAsync, () => SelectedExtractModel is not null);
            UpdateExtractModelCommand = new AsyncRelayCommand(UpdateExtractModelAsync, () => SelectedExtractModel?.Id > _fixedIdRange);
            RemoveExtractModelCommand = new AsyncRelayCommand(RemoveExtractModelAsync, () => SelectedExtractModel?.Id > _fixedIdRange);

            AddLoraModelCommand = new AsyncRelayCommand(AddLoraModelAsync);
            CopyLoraModelCommand = new AsyncRelayCommand(CopyLoraModelAsync, () => SelectedLoraModel is not null);
            UpdateLoraModelCommand = new AsyncRelayCommand(UpdateLoraModelAsync, () => SelectedLoraModel?.Id > _fixedIdRange);
            RemoveLoraModelCommand = new AsyncRelayCommand(RemoveLoraModelAsync, () => SelectedLoraModel?.Id > _fixedIdRange);

            AddUpscaleModelCommand = new AsyncRelayCommand(AddUpscaleModel);
            CopyUpscaleModelCommand = new AsyncRelayCommand(CopyUpscaleModelAsync, () => SelectedUpscaleModel is not null);
            UpdateUpscaleModelCommand = new AsyncRelayCommand(UpdateUpscaleModelAsync, () => SelectedUpscaleModel?.Id > _fixedIdRange);
            RemoveUpscaleModelCommand = new AsyncRelayCommand(RemoveUpscaleModelAsync, () => SelectedUpscaleModel?.Id > _fixedIdRange);

            EnvironmentServiceCreateCommand = new AsyncRelayCommand(EnvironmentCreateAsync, CanEnvironmentCreate);
            EnvironmentServiceUpdateCommand = new AsyncRelayCommand(EnvironmentUpdateAsync, CanEnvironmentUpdate);
            EnvironmentServiceRebuildCommand = new AsyncRelayCommand(EnvironmentRebuildAsync, CanEnvironmentUpdate);
            EnvironmentServiceDeleteCommand = new AsyncRelayCommand(EnvironmentDeleteAsync, CanEnvironmentUpdate);

            SelectedEnvironment = settings.Environments.FirstOrDefault();
            InitializeComponent();
        }

        public override int Id => (int)View.Settings;
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand AddEnvironmentCommand { get; }
        public AsyncRelayCommand CopyEnvironmentCommand { get; }
        public AsyncRelayCommand UpdateEnvironmentCommand { get; }
        public AsyncRelayCommand RemoveEnvironmentCommand { get; }
        public AsyncRelayCommand AddDiffusionModelCommand { get; }
        public AsyncRelayCommand CopyDiffusionModelCommand { get; }
        public AsyncRelayCommand UpdateDiffusionModelCommand { get; }
        public AsyncRelayCommand RemoveDiffusionModelCommand { get; }
        public AsyncRelayCommand AddControlNetModelCommand { get; }
        public AsyncRelayCommand CopyControlNetModelCommand { get; }
        public AsyncRelayCommand UpdateControlNetModelCommand { get; }
        public AsyncRelayCommand RemoveControlNetModelCommand { get; }
        public AsyncRelayCommand AddExtractModelCommand { get; }
        public AsyncRelayCommand CopyExtractModelCommand { get; }
        public AsyncRelayCommand UpdateExtractModelCommand { get; }
        public AsyncRelayCommand RemoveExtractModelCommand { get; }
        public AsyncRelayCommand AddLoraModelCommand { get; }
        public AsyncRelayCommand CopyLoraModelCommand { get; }
        public AsyncRelayCommand UpdateLoraModelCommand { get; }
        public AsyncRelayCommand RemoveLoraModelCommand { get; }
        public AsyncRelayCommand AddUpscaleModelCommand { get; }
        public AsyncRelayCommand UpdateUpscaleModelCommand { get; }
        public AsyncRelayCommand CopyUpscaleModelCommand { get; }
        public AsyncRelayCommand RemoveUpscaleModelCommand { get; }


        public AsyncRelayCommand EnvironmentServiceCreateCommand { get; }
        public AsyncRelayCommand EnvironmentServiceUpdateCommand { get; }
        public AsyncRelayCommand EnvironmentServiceRebuildCommand { get; }
        public AsyncRelayCommand EnvironmentServiceDeleteCommand { get; }

        public EnvironmentModel SelectedEnvironment
        {
            get { return _selectedEnvironment; }
            set { SetProperty(ref _selectedEnvironment, value); }
        }

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

        public ExtractModel SelectedExtractModel
        {
            get { return _selectedExtractModel; }
            set { SetProperty(ref _selectedExtractModel, value); }
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

        private async Task AddEnvironmentAsync()
        {
            var dialog = DialogService.GetDialog<EnvironmentModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }

        private async Task CopyEnvironmentAsync()
        {
            var dialog = DialogService.GetDialog<EnvironmentModelDialog>();
            if (await dialog.CopyAsync(SelectedEnvironment))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateEnvironmentAsync()
        {
            var dialog = DialogService.GetDialog<EnvironmentModelDialog>();
            if (await dialog.UpdateAsync(SelectedEnvironment))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveEnvironmentAsync()
        {
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.Environments.Remove(SelectedEnvironment);
                SelectedEnvironment = default;
                await SaveAsync();
            }
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
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.DiffusionModels.Remove(SelectedDiffusionModel);
                SelectedDiffusionModel = default;
                await SaveAsync();
            }
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
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.ControlNetModels.Remove(SelectedControlNetModel);
                SelectedControlNetModel = default;
                await SaveAsync();
            }
        }


        private async Task AddExtractModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractModelDialog>();
            if (await dialog.AddAsync())
            {
                await SaveAsync();
            }
        }


        private async Task CopyExtractModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractModelDialog>();
            if (await dialog.CopyAsync(SelectedExtractModel))
            {
                await SaveAsync();
            }
        }


        private async Task UpdateExtractModelAsync()
        {
            var dialog = DialogService.GetDialog<ExtractModelDialog>();
            if (await dialog.UpdateAsync(SelectedExtractModel))
            {
                await SaveAsync();
            }
        }


        private async Task RemoveExtractModelAsync()
        {
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.ExtractModels.Remove(SelectedExtractModel);
                SelectedExtractModel = default;
                await SaveAsync();
            }
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
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.LoraAdapterModels.Remove(SelectedLoraModel);
                SelectedLoraModel = default;
                await SaveAsync();
            }
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
            if (await dialog.CopyAsync(SelectedUpscaleModel))
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
            if (await DialogService.ShowMessageAsync("Delete Model", $"Are you sure you want to delete this model?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                Settings.UpscaleModels.Remove(SelectedUpscaleModel);
                SelectedUpscaleModel = default;
                await SaveAsync();
            }
        }


        private async Task EnvironmentCreateAsync()
        {
            var environmentDialog = DialogService.GetDialog<EnvironmentDialog>();
            await environmentDialog.CreateAsync(SelectedEnvironment);
        }


        private bool CanEnvironmentCreate()
        {
            if (SelectedEnvironment is null)
                return false;

            return !EnvironmentService.Exists(SelectedEnvironment);
        }


        private async Task EnvironmentUpdateAsync()
        {
            var environmentDialog = DialogService.GetDialog<EnvironmentDialog>();
            await environmentDialog.UpdateAsync(SelectedEnvironment);
        }


        private bool CanEnvironmentUpdate()
        {
            if (SelectedEnvironment is null)
                return false;

            return EnvironmentService.Exists(SelectedEnvironment);
        }


        private async Task EnvironmentRebuildAsync()
        {
            var environmentDialog = DialogService.GetDialog<EnvironmentDialog>();
            await environmentDialog.RebuildAsync(SelectedEnvironment);
        }


        private async Task EnvironmentDeleteAsync()
        {
            if (await DialogService.ShowMessageAsync("Delete Environment", $"Are you sure you want to delete this python environment?", TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Warning, TensorStack.WPF.Dialogs.MessageBoxStyleType.Danger))
            {
                await EnvironmentService.DeleteAsync(SelectedEnvironment);
            }
        }


        private async Task SaveAsync()
        {
            await Json.SaveAsync<Settings>("Settings.json", Settings);
            Settings.ScanModels();
        }
    }
}