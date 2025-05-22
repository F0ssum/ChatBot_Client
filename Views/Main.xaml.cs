using System;
using System.Windows;
using ChatBotClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ChatBotClient.Views
{
	public partial class Main : Window
	{
		private readonly MainViewModel _viewModel;
		private static bool _isInitialized;

		public Main(IServiceProvider serviceProvider)
		{
			if (_isInitialized)
			{
				Log.Warning("Main window initialization attempted again, skipping");
				return;
			}

			try
			{
				Log.Information("Starting Main window initialization");
				_isInitialized = true;
				InitializeComponent();

				Opacity = 1;
				Visibility = Visibility.Visible;
				WindowState = WindowState.Normal;
				WindowStartupLocation = WindowStartupLocation.CenterScreen;

				Log.Information("Main window properties set: Visibility={Visibility}, Opacity={Opacity}, Position={Left},{Top}, State={WindowState}",
					Visibility, Opacity, Left, Top, WindowState);

				_viewModel = serviceProvider.GetRequiredService<MainViewModel>();
				DataContext = _viewModel;
				Log.Information("Main window initialized successfully, DataContext: {DataContext}", _viewModel.GetType().Name);

				Loaded += (s, e) =>
				{
					Log.Information("Main window Loaded event triggered");
					_viewModel.InitializeNavigation(this);
				};
				Activated += (s, e) => Log.Information("Main window Activated event triggered");
				Closed += (s, e) => Log.Information("Main window Closed event triggered");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize Main window: {Message}", ex.Message);
				MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
			}
		}
	}
}