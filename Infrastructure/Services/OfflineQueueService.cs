using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using Newtonsoft.Json;
using Serilog;
using System.IO;

namespace ChatBotClient.Infrastructure.Services
{

	public class OfflineQueueService
	{
		private readonly string _queueFile;
		private readonly object _lock = new object();

		public OfflineQueueService(AppConfiguration config)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string basePath = Path.Combine(appDataPath, config.AppName);
			Directory.CreateDirectory(basePath);
			_queueFile = Path.Combine(basePath, "offline_queue.json");
			Log.Information("OfflineQueueService initialized with queueFile: {QueueFile}", _queueFile);
		}

		public void QueueAction(string action, object data)
		{
			lock (_lock)
			{
				try
				{
					var queue = LoadQueue() ?? new List<OfflineQueueItem>();
					queue.Add(new OfflineQueueItem { Action = action, Data = data, Timestamp = DateTime.Now });
					SaveQueue(queue);
					Log.Information("Queued offline action: {Action}", action);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error queuing action: {Action}", action);
					throw;
				}
			}
		}

		public async Task SyncQueueAsync(ApiService apiService)
		{
			List<OfflineQueueItem> queue;

			// Блокировка только для синхронных операций
			lock (_lock)
			{
				queue = LoadQueue();
				if (queue == null || !queue.Any())
					return;
			}

			try
			{
				foreach (var item in queue)
				{
					try
					{
						if (item.Action == "SendMessage")
						{
							var payload = JsonConvert.DeserializeObject<dynamic>(item.Data.ToString());
							await apiService.SendMessageAsync(
								(string)payload.userId,
								(string)payload.message,
								JsonConvert.DeserializeObject<List<Message>>(payload.history.ToString()),
								(string)payload.language,
								(string)payload.customPrompt,
								(double)payload.temperature,
								(double)payload.topP,
								(int)payload.maxResponseLength
							);
						}

						if (item.Action == "SendAudio")
						{
							var audioData = JsonConvert.DeserializeObject<(string userId, string filePath)>(item.Data.ToString());
							await apiService.SendAudioAsync(audioData.userId, audioData.filePath);
							queue.Remove(item);
						}
						else if (item.Action == "CreateDiaryEntry")
						{
							var payload = JsonConvert.DeserializeObject<dynamic>(item.Data.ToString());
							await apiService.CreateDiaryEntryAsync(
								(string)payload.userId,
								JsonConvert.DeserializeObject<DiaryEntry>(payload.entry.ToString())
							);
						}
						Log.Information("Synced offline action: {Action}", item.Action);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to sync offline action: {Action}", item.Action);
					}
				}

				// Блокировка для сохранения изменений
				lock (_lock)
				{
					SaveQueue(new List<OfflineQueueItem>());
				}

				Log.Information("Offline queue synced and cleared");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error syncing offline queue");
				throw;
			}
		}

		private List<OfflineQueueItem> LoadQueue()
		{
			if (!File.Exists(_queueFile))
				return null;

			string json = File.ReadAllText(_queueFile);
			return JsonConvert.DeserializeObject<List<OfflineQueueItem>>(json);
		}

		private void SaveQueue(List<OfflineQueueItem> queue)
		{
			string json = JsonConvert.SerializeObject(queue, Formatting.Indented);
			File.WriteAllText(_queueFile, json);
		}
	}
}