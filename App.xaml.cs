using ChatBotClient.Services;
using ChatBotClient.ViewModels;
using ChatBotClient.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows;
using System.Windows.Media;

namespace ChatBotClient
{
	public partial class App : Application
	{
		private readonly IServiceProvider _serviceProvider;
		private static bool _isMainWindowCreated;

		public App()
		{
			var services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();
			Log.Information("Dependency injection container initialized");
		}

		private void ConfigureServices(ServiceCollection services)
		{
			services.AddSingleton<ApiService>();
			services.AddSingleton<LocalStorageService>();
			services.AddTransient<ChatViewModel>();
			services.AddTransient<DiaryViewModel>();
			services.AddTransient<SettingsViewModel>();
			services.AddTransient<MainViewModel>();
			services.AddTransient<Main>(); // Используем Transient, чтобы избежать повторной инициализации
			services.AddTransient<ChatPage>();
			services.AddTransient<DiaryPage>();
			services.AddTransient<SettingsPage>();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
			if (processes.Length > 1)
			{
				Log.Warning("Multiple instances of {ProcessName} detected, shutting down", currentProcess.ProcessName);
				Shutdown();
				return;
			}

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
				.CreateLogger();
			Log.Information("Application starting up, Process ID: {ProcessId}", currentProcess.Id);

			base.OnStartup(e);

			DispatcherUnhandledException += (s, args) =>
			{
				Log.Error(args.Exception, "Unhandled exception: {Message}\nFull StackTrace: {StackTrace}", args.Exception.Message, args.Exception.StackTrace);
				MessageBox.Show($"Ошибка: {args.Exception.Message}\n{args.Exception.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				args.Handled = true;
			};

			try
			{
				if (_isMainWindowCreated)
				{
					Log.Warning("Main window creation attempted again, skipping");
					return;
				}

				Log.Information("Creating Main window");
				var mainWindow = _serviceProvider.GetRequiredService<Main>();
				_isMainWindowCreated = true;

				Log.Information("Main window created, showing window");
				RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
				mainWindow.Show();
				mainWindow.Activate();
				Log.Information("Main window displayed successfully, Visibility: {Visibility}, Opacity: {Opacity}, Position: {Left},{Top}, State: {WindowState}",
					mainWindow.Visibility, mainWindow.Opacity, mainWindow.Left, mainWindow.Top, mainWindow.WindowState);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to start application: {Message}\nFull StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
				MessageBox.Show($"Failed to start application: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Log.Information("Application shutting down");
			try
			{
				Log.CloseAndFlush();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to flush logs: {Message}", ex.Message);
			}
			base.OnExit(e);
		}
	}
}