namespace ChatBotClient.Core.Models
{
	public class DiaryEntry
	{
		private string _title;
		private string _content;

		public string Title
		{
			get => _title;
			set => _title = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Title cannot be empty");
		}

		public DateTime Date { get; set; }

		public string Content
		{
			get => _content;
			set => _content = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Content cannot be empty");
		}

		public List<string> Tags { get; set; } = new List<string>();

		// 🔽 Новое свойство
		public string Emoji { get; set; }

		public DiaryEntry()
		{
			Date = DateTime.Now;
		}
	}
}