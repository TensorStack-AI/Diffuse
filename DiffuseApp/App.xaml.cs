using Diffuse.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TensorStack.WPF;

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

        private static IHost _appHost;
        private readonly Splashscreen _splashscreen = new();
        public App()
        {
            RegisterExceptionHandlers();

            ConfigureLogging();
            var builder = Host.CreateApplicationBuilder();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger: Log.Logger, dispose: true);

            var configuration = Json.Load<Settings>("Settings.json");
            configuration.Initialize();

            // Add WPFCommon
            builder.Services.AddWPFCommon<MainWindow, Settings>(configuration);

            builder.Services.AddSingleton<IHardwareService, HardwareService>();
            builder.Services.AddSingleton<IMediaService, MediaService>();
            builder.Services.AddSingleton<IHistoryService, HistoryService>();
            builder.Services.AddSingleton<IUpscaleService, UpscaleService>();
            builder.Services.AddSingleton<IExtractorService, ExtractorService>();
            builder.Services.AddSingleton<IDiffusionService, DiffusionService>();
            builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();

            _appHost = builder.Build();

            // Initialize WPFCommon
            _appHost.Services.UseWPFCommon();
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public static T GetService<T>() => _appHost.Services.GetService<T>();


        /// <summary>
        /// Application startup.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task AppStartup()
        {
            var historyService = _appHost.Services.GetService<IHistoryService>();
            await historyService.InitializeAsync();

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
                await _appHost.StopAsync();
                DeregisterExceptionHandlers();
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
        private void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowExceptionMessage(e.Exception);

            // Prevent application from crashing
            e.Handled = true;
        }


        /// <summary>
        /// Handles the <see cref="E:AppDomainException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void OnAppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }


        /// <summary>
        /// Handles the <see cref="E:TaskSchedulerException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void OnTaskSchedulerException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowExceptionMessage(e.Exception);

            // Prevent application from crashing
            e.SetObserved();
        }


        private void ShowExceptionMessage(Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private static string GetAppVersionDisplay()
        {
            return $"v{AppVersion}-alpha";
        }

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
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"Logs\Diffuse-{DateTime.Now.ToString("dd-MM-yyyy")}-{now.Hour * 3600 + now.Minute * 60 + now.Second}.txt");
        }
    }
}


