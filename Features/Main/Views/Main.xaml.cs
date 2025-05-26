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

		public Main(IServiceProvider serviceProvider)
		{
			if (_isInitialized)
				return;
			_isInitialized = true;

			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_navigationService = serviceProvider.GetService<NavigationService>() ?? throw new InvalidOperationException("NavigationService not registered");
			InitializeComponent();
			DataContext = serviceProvider.GetService<MainViewModel>();
			_navigationService.SetMainFrame(MainFrame);
		}
	}
}