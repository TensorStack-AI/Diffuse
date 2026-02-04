using Diffuse.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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
            ScaleOptions = [.. Enumerable.Range(5, 26) .Select(x => new ScaleOption($"{x * 10}%", x / 10.0))];
            InitializeComponent();
        }

        public override View View => View.General;
        public AsyncRelayCommand SaveCommand { get; }
        public IReadOnlyList<ScaleOption> ScaleOptions { get; }

        private async Task SaveAsync()
        {
            await SettingsManager.SaveAsync(Settings);
            Settings.ScanModels();
        }

        public record ScaleOption(string Label, double Value);
    }
}