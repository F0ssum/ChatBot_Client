using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Infrastructure.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace ChatBotClient.Features.Tree
{
	public partial class TreeViewModel : ObservableObject
	{
		private readonly NavigationService _navigationService;

		public TreeViewModel(NavigationService navigationService)
		{
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			Log.Information("TreeViewModel initialized");
		}

		[RelayCommand]
		private void GoToChat()
		{
			_navigationService.NavigateTo<ChatPage>();
			Log.Information("Navigated to ChatPage");
		}
	}
}