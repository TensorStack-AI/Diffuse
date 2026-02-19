using Diffuse.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TensorStack.Common.Common;
using TensorStack.WPF;
using TensorStack.WPF.Services;

namespace Diffuse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string AppName = "Diffuse";                          // Diffuse
        public static readonly string AppVersion = GetAppVersion();                 // 0.3.0
        public static readonly string AppVersionDisplay = GetAppVersionDisplay();   // v0.3.0
        public static readonly string AppDisplayName = GetAppDisplayName();         // Diffuse v0.3.0
        private readonly Splashscreen _splashscreen = new();
        private static IHost _appHost;
        private static Mutex _appMutex;
        private static string _directoryBase;
        private static string _directoryData;
        private static string _directoryPython;
        private readonly Settings _settings;

        public App()
        {
            _appMutex = new Mutex(false, "Global\\TensorStack_Diffuse", out bool isNewInstance);
            if (!isNewInstance)
            {
                ActivateExistingInstance();
                return;
            }

            RegisterExceptionHandlers();

            // Paths
            _directoryBase = AppDomain.CurrentDomain.BaseDirectory;
            _directoryData = GetApplicationDataDirectory();
            _directoryPython = Path.Combine(_directoryData, "PythonRuntime");

            // Host
            var builder = Host.CreateApplicationBuilder();

            // Logging
            ConfigureLogging();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger: Log.Logger, dispose: true);

            // Add TensorStack.WPF
            _settings = LoadSettingsFile();
            builder.Services.AddWPFCommon<MainWindow, Settings>(_settings);

            // Add sService
            builder.Services.AddSingleton<IHardwareService, HardwareService>();
            builder.Services.AddSingleton<IMediaService, MediaService>();
            builder.Services.AddSingleton<IHistoryService, HistoryService>();
            builder.Services.AddSingleton<IUpscaleService, UpscaleService>();
            builder.Services.AddSingleton<IExtractService, ExtractService>();
            builder.Services.AddSingleton<IDiffusionService, DiffusionService>();
            builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
            builder.Services.AddSingleton<IInterpolationService, InterpolationService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();

            // Build
            _appHost = builder.Build();

            // TensorStack.WPF
            _appHost.Services.UseWPFCommon();
        }

        public static string DirectoryBase => _directoryBase;
        public static string DirectoryData => _directoryData;
        public static string DirectoryPython => _directoryPython;
        public static string DirectoryServer => _directoryBase;


        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public static T GetService<T>() => _appHost.Services.GetService<T>();


        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        private static string GetApplicationDataDirectory()
        {
#if RELEASE_INSTALLER
             return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Diffuse");
#else
            return _directoryBase;
#endif
        }


        /// <summary>
        /// Loads the settings file.
        /// </summary>
        private static Settings LoadSettingsFile()
        {
            var configuration = SettingsManager.Load();
            configuration.Initialize(_directoryData);
            return configuration;
        }


        /// <summary>
        /// Application startup.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task AppStartup()
        {
            var historyService = _appHost.Services.GetService<IHistoryService>();
            var hardwareService = _appHost.Services.GetService<IHardwareService>();

            // Load History
            await historyService.InitializeAsync();

            // Load Devices
            var devices = hardwareService.GetGPUDevices();
            _settings.InitializeDevices(devices);

            // Open Main Window
            MainWindow = _appHost.Services.GetMainWindow();
            MainWindow.Show();
            _splashscreen.Close();
        }


        /// <summary>
        /// Application shutdown.
        /// </summary>
        private async Task AppShutdown()
        {
            using (_appHost)
            {
                await SettingsManager.SaveAsync(_settings);
                await _appHost.StopAsync();
                DeregisterExceptionHandlers();
                _appMutex.WaitOne();
                _appMutex.ReleaseMutex();
                _appMutex.Dispose();
                FileQueue.Shutdown();
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppStartup();
            base.OnStartup(e);
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.SessionEnding" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.SessionEndingCancelEventArgs" /> that contains the event data.</param>
        protected override async void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            await AppShutdown();
            base.OnSessionEnding(e);
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected async override void OnExit(ExitEventArgs e)
        {
            await AppShutdown();
            base.OnExit(e);
        }


        /// <summary>
        /// Registers the exception handlers.
        /// </summary>
        private void RegisterExceptionHandlers()
        {
            DispatcherUnhandledException += OnDispatcherException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerException;
        }


        /// <summary>
        /// Deregisters the exception handlers.
        /// </summary>
        private void DeregisterExceptionHandlers()
        {
            DispatcherUnhandledException -= OnDispatcherException;
            AppDomain.CurrentDomain.UnhandledException -= OnAppDomainException;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerException;
        }


        /// <summary>
        /// Handles the <see cref="E:DispatcherException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private async void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            await ShowExceptionMessage(e.Exception);

            // Prevent application from crashing
            e.Handled = true;
        }


        /// <summary>
        /// Handles the <see cref="E:AppDomainException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private async void OnAppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                await ShowExceptionMessage(ex);
            }
        }


        /// <summary>
        /// Handles the <see cref="E:TaskSchedulerException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private async void OnTaskSchedulerException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await ShowExceptionMessage(e.Exception);

            // Prevent application from crashing
            e.SetObserved();
        }


        /// <summary>
        /// Shows the exception message.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private static async Task ShowExceptionMessage(Exception ex)
        {
            Log.Logger.Error(ex, "[Application] [Exception] An unexpected exception occurred.");
            await DialogService.ShowErrorAsync("Unexpected Error", $"An unexpected error occurred:\n{ex.Message}");
        }


        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }


        /// <summary>
        /// Gets the application version display name.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetAppVersionDisplay()
        {
            return $"v{AppVersion}-beta";
        }


        /// <summary>
        /// Gets the display name of the application.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetAppDisplayName()
        {
            return $"{AppName} {AppVersionDisplay}";
        }


        /// <summary>
        /// Configures the logging.
        /// </summary>
        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(GetLogName(), rollOnFileSizeLimit: true)
                .CreateLogger();
        }


        /// <summary>
        /// Gets the name of the log.
        /// </summary>
        private static string GetLogName()
        {
            var now = DateTime.Now;
            return Path.Combine(_directoryData, @$"Logs\Diffuse-{DateTime.Now.ToString("dd-MM-yyyy")}-{now.Hour * 3600 + now.Minute * 60 + now.Second}.txt");
        }


        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void ActivateExistingInstance()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
            Environment.Exit(0);
        }
    }
}


