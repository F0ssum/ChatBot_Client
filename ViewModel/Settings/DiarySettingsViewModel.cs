using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace ChatBotClient.ViewModel.Settings
{
	public partial class DiarySettingsViewModel : ObservableObject
	{
		private int _diaryNoteStyleIndex;

		public int DiaryNoteStyleIndex
		{
			get => _diaryNoteStyleIndex;
			set => SetProperty(ref _diaryNoteStyleIndex, value);
		}

		public DiarySettingsViewModel()
		{
			Log.Information("DiarySettingsViewModel initialized");
		}

		public void Reset()
		{
			DiaryNoteStyleIndex = 0;
			Log.Information("DiarySettingsViewModel reset");
		}
	}
}