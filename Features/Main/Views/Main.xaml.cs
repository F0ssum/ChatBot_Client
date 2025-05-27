using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ChatBotClient.Features.Main.Views
{
	public partial class Main : Window
	{
		private static bool _isInitialized;
		private readonly IServiceProvider _serviceProvider;
		private readonly NavigationService _navigationService;
		private readonly MainViewModel _viewModel;

		public Main(IServiceProvider serviceProvider)
		{
			if (_isInitialized)
			{
				Close();
				return;
			}
			_isInitialized = true;

			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_navigationService = serviceProvider.GetService<NavigationService>() ?? throw new InvalidOperationException("NavigationService not registered");
			_viewModel = serviceProvider.GetService<MainViewModel>() ?? throw new InvalidOperationException("MainViewModel not registered");
			InitializeComponent();
			DataContext = _viewModel;
			_navigationService.SetMainFrame(MainFrame);
			Loaded += Main_Loaded;
		}

		private async void Main_Loaded(object sender, RoutedEventArgs e)
		{
			await _viewModel.InitializeAsync();
		}
	}
}