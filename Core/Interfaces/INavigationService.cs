// Core/Interfaces/INavigationService.cs
using System.Windows.Controls;

namespace ChatBotClient.Core
{
	/// <summary>
	/// Provides methods for navigating between pages in the application.
	/// </summary>
	public interface INavigationService
	{
		/// <summary>
		/// Sets the main frame for navigation.
		/// </summary>
		/// <param name="frame">The frame to use for navigation.</param>
		void SetMainFrame(Frame frame);

		/// <summary>
		/// Navigates to a specified page type.
		/// </summary>
		/// <typeparam name="TPage">The type of page to navigate to.</typeparam>
		void NavigateTo<TPage>() where TPage : Page;
	}
}