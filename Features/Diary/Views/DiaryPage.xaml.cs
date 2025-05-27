using ChatBotClient.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.Features.Diary.Views
{
	public partial class DiaryPage : Page
	{
		private readonly DiaryViewModel _viewModel;

		public DiaryPage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				_viewModel = serviceProvider.GetRequiredService<DiaryViewModel>();
				DataContext = _viewModel;
				Log.Information("DiaryPage initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize DiaryPage: {Message}", ex.Message);
				MessageBox.Show($"Failed to initialize diary: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void OpenEntry(int entryId)
		{
			var entry = _viewModel.Entries.FirstOrDefault(e => e.Date.Ticks.GetHashCode() == entryId);
			if (entry != null)
			{
				// Можно выделить запись, открыть модальное окно или прокрутить к ней
				_viewModel.CreateNoteModalVisibility = true;
				_viewModel.NewNoteContent = entry.Content;
				_viewModel.SelectedEmoji = entry.Emoji;
				// Можно добавить дополнительные поля для отображения
			}
		}
	}
}