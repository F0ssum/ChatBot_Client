using ChatBotClient.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace ChatBotClient.Infrastructure.Services
{
	public class NavigationService : INavigationService
	{
		private Frame _mainFrame;
		private readonly IServiceProvider _serviceProvider;

		public NavigationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void SetMainFrame(Frame frame)
		{
			_mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
		}

		public void NavigateTo<TPage>() where TPage : Page
		{
			if (_mainFrame == null)
				throw new InvalidOperationException("MainFrame не установлен");

			var page = _serviceProvider.GetService<TPage>();
			if (page == null)
				throw new InvalidOperationException($"Страница {typeof(TPage)} не зарегистрирована");

			_mainFrame.Navigate(page);
		}
	}
}