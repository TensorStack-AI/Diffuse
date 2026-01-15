using Diffuse.Services;
using System.Threading.Tasks;
using TensorStack.WPF;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for SettingsGeneralView.xaml
    /// </summary>
    public partial class SettingsGeneralView : ViewBase
    {
        public SettingsGeneralView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(settings, navigationService, environmentService, historyService)
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            InitializeComponent();
        }

        public override int Id => (int)View.General;
        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveAsync()
        {
            await SettingsManager.SaveAsync(Settings);
            Settings.ScanModels();
        }
    }
}