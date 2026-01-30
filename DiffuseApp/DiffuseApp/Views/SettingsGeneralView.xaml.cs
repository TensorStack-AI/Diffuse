using Diffuse.Services;
using Microsoft.Extensions.Logging;
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
        public SettingsGeneralView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService, ILogger<SettingsGeneralView> logger)
            : base(settings, navigationService, environmentService, historyService, logger)
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            InitializeComponent();
        }

        public override View View => View.General;
        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveAsync()
        {
            await SettingsManager.SaveAsync(Settings);
            Settings.ScanModels();
        }
    }
}