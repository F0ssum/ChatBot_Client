using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBotClient.Infrastructure.Services
{
	public class OfflineQueueService
	{
		private readonly string _queueFile;
		private readonly object _lock = new();
		private readonly LocalStorageService _localStorageService;

		public OfflineQueueService(AppConfiguration config, LocalStorageService localStorageService)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string basePath = Path.Combine(appDataPath, config.AppName);
			Directory.CreateDirectory(basePath);
			_queueFile = Path.Combine(basePath, "offline_queue.json");
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
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

			lock (_lock)
			{
				queue = LoadQueue();
				if (queue == null || !queue.Any())
				{
					Log.Information("No actions in offline queue to sync");
					return;
				}
			}

			try
			{
				foreach (var item in queue.ToList()) // Создаем копию для итерации
				{
					try
					{
						if (item.Action == "SendMessage")
						{
							var payload = JsonConvert.DeserializeObject<dynamic>(item.Data.ToString());
							var history = JsonConvert.DeserializeObject<List<Message>>(payload.history.ToString());
							string response = await apiService.SendMessageAsync(
								(string)payload.userId,
								(string)payload.message,
								history,
								(string)payload.language,
								(string)payload.customPrompt,
								(double)payload.temperature,
								(double)payload.topP,
								(int)payload.maxResponseLength
							);

							// Сохраняем сообщение и ответ в локальной истории
							history.Add(new Message { Author = "User", Text = (string)payload.message, Timestamp = DateTime.Now });
							history.Add(new Message { Author = "Bot", Text = response, Timestamp = DateTime.Now });
							await _localStorageService.SaveHistoryAsync((string)payload.userId, history);

							queue.Remove(item);
							Log.Information("Synced offline action: SendMessage for user {UserId}", (string)payload.userId);
						}
						else if (item.Action == "SendAudio")
						{
							var audioData = JsonConvert.DeserializeObject<(string userId, string filePath)>(item.Data.ToString());
							string response = await apiService.SendAudioAsync(audioData.userId, audioData.filePath);

							// Сохраняем аудио-сообщение и ответ в локальной истории
							var history = await _localStorageService.GetHistoryAsync(audioData.userId) ?? new List<Message>();
							history.Add(new Message { Author = "User", Text = "[Audio Message]", Timestamp = DateTime.Now });
							history.Add(new Message { Author = "Bot", Text = response, Timestamp = DateTime.Now });
							await _localStorageService.SaveHistoryAsync(audioData.userId, history);

							queue.Remove(item);
							Log.Information("Synced offline action: SendAudio for user {UserId}", audioData.userId);
						}
						else
						{
							Log.Warning("Unknown action in queue: {Action}", item.Action);
							queue.Remove(item);
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to sync offline action: {Action}", item.Action);
						// Продолжаем обработку других элементов в очереди
					}
				}

				lock (_lock)
				{
					SaveQueue(queue);
				}

				Log.Information("Offline queue synced successfully");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error syncing offline queue");
				throw;
			}
		}

		private List<OfflineQueueItem> LoadQueue()
		{
			lock (_lock)
			{
				if (!File.Exists(_queueFile))
					return null;

				try
				{
					string json = File.ReadAllText(_queueFile);
					return JsonConvert.DeserializeObject<List<OfflineQueueItem>>(json);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error loading offline queue from {QueueFile}", _queueFile);
					return null;
				}
			}
		}

		private void SaveQueue(List<OfflineQueueItem> queue)
		{
			lock (_lock)
			{
				try
				{
					string json = JsonConvert.SerializeObject(queue, Formatting.Indented);
					File.WriteAllText(_queueFile, json);
					Log.Information("Saved offline queue to {QueueFile}", _queueFile);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error saving offline queue to {QueueFile}", _queueFile);
					throw;
				}
			}
		}
	}
}