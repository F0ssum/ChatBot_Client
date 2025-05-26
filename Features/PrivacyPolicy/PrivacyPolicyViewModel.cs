using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Infrastructure.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace ChatBotClient.Features.PrivacyPolicy
{
	public partial class PrivacyPolicyViewModel : ObservableObject
	{
		private readonly NavigationService _navigationService;
		private string _policyText;

		public string PolicyText
		{
			get => _policyText;
			set => SetProperty(ref _policyText, value);
		}

		public PrivacyPolicyViewModel(NavigationService navigationService)
		{
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			PolicyText = "Здесь будет текст политики конфиденциальности, загруженный с сервера или локального ресурса.";
			Log.Information("PrivacyPolicyViewModel initialized");
		}

		[RelayCommand]
		private void GoToChat()
		{
			_navigationService.NavigateTo<ChatPage>();
			Log.Information("Navigated to ChatPage");
		}
	}
}