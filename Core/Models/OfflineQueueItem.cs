namespace ChatBotClient.Core.Models
{
	public class OfflineQueueItem
	{
		public string Action { get; set; }
		public object Data { get; set; }
		public DateTime Timestamp { get; set; }
	}
}