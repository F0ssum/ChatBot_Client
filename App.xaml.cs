using System.Windows;
using ChatBotClient.Core.Configuration;
using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Features.Diary;
using ChatBotClient.Features.Diary.Views;
using ChatBotClient.Features.Main;
using ChatBotClient.Features.Main.Views;
using ChatBotClient.Features.Services;
using ChatBotClient.Features.Settings;
using ChatBotClient.Features.Settings.Views;
using ChatBotClient.Features.Tree;
using ChatBotClient.Features.Tree.Views;
using ChatBotClient.Infrastructure.Service;
using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel;
using ChatBotClient.ViewModel.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace ChatBotClient
{
	public partial class Application : System.Windows.Application
	{
		private readonly IServiceProvider _serviceProvider;

		public Application()
		{
			var services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();

			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
				.CreateLogger();
		}

		private void ConfigureServices(ServiceCollection services)
		{
			// Configuration
			services.AddSingleton<AppConfiguration>(new AppConfiguration
			{
				ApiBaseUrl = "http://localhost:5000",
				LocalModelPath = "Resources/Models/model.bin"
			});

			// Services
			services.AddSingleton<NavigationService>();
			services.AddSingleton<LocalStorageService>();
			services.AddSingleton<ApiService>();
			services.AddSingleton<NotificationService>();
			services.AddSingleton<OfflineQueueService>();
			services.AddSingleton<AnalyticsService>();

			// ViewModels
			services.AddTransient<MainViewModel>();
			services.AddTransient<ChatViewModel>();
			services.AddTransient<DiaryViewModel>();
			services.AddTransient<SettingsViewModel>();
			services.AddTransient<ChatSettingsViewModel>();
			services.AddTransient<DiarySettingsViewModel>();
			services.AddTransient<ProfileSettingsViewModel>();
			services.AddTransient<ModelSettingsViewModel>();
			services.AddTransient<NotificationSettingsViewModel>();
			services.AddTransient<TreeViewModel>();

			// Views
			services.AddTransient<Main>();
			services.AddTransient<ChatPage>();
			services.AddTransient<DiaryPage>();
			services.AddTransient<SettingsPage>();
			services.AddTransient<TreePage>();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			try
			{
				var mainWindow = _serviceProvider.GetService<Main>();
				mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();
				mainWindow.Show();
				Log.Information("Application started");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to start application");
				MessageBox.Show($"Failed to start application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Log.Information("Application exiting");
			Log.CloseAndFlush();
			base.OnExit(e);
		}
	}
}