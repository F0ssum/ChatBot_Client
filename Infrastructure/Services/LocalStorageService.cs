using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using ChatBotClient.Infrastructure.Interfaces;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotClient.Infrastructure.Services
{
	public interface ILocalStorageService
	{
		List<string> GetUserIds();
		void SaveUserIds(IList<string> userIds);
		(List<string> userIds, string token) LoadUserData();
		void SaveData<T>(T data);
		T LoadData<T>(string userId = null);
		Task SaveDataAsync<T>(string key, T data);
		Task<T> LoadDataAsync<T>(string key);
		Task<bool> ContainsDataAsync(string key);
		Task RemoveDataAsync(string key);
		Task ClearAllDataAsync();
		Task SaveAvatarAsync(string userId, string filePath);
		Task CacheDataAsync<T>(string key, T data, TimeSpan expiry);
		Task<T> GetCachedDataAsync<T>(string key);
		Task ClearCacheAsync(string keyPrefix);
		Task<List<Message>> GetHistoryAsync(string userId, int page = 1, int pageSize = 50);
		Task SaveHistoryAsync(string userId, List<Message> history);
		Task<Dictionary<string, int>> GetMoodStatsAsync(string userId);
		Task SaveMoodStatsAsync(string userId, Dictionary<string, int> stats);
		Task<List<string>> GetChatHistoryListAsync(string userId);
		Task SaveChatHistoryListAsync(string userId, List<string> list);
		Task ClearChatHistoryAsync(string userId);
		Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId, int page = 1, int pageSize = 50);
		Task CreateDiaryEntryAsync(string userId, DiaryEntry entry);
		Task<List<string>> GetDiaryTagsAsync(string userId);
		Task AddDiaryTagAsync(string userId, string tag);
		Task ArchiveDiaryEntriesAsync(string userId);
		Task<List<string>> GetTriggersAsync(string userId);
		Task AddTriggerAsync(string userId, string trigger);
		Task<string> CreateUserIdAsync(string username); // Added new method
	}

	public class LocalStorageService : ILocalStorageService
	{
		private readonly string _userDataFile;
		private readonly object _lock = new();

		public LocalStorageService(AppConfiguration config)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string basePath = Path.Combine(appDataPath, config.AppName);
			Directory.CreateDirectory(basePath);
			_userDataFile = Path.Combine(basePath, "user_data.json");
			Log.Information("LocalStorageService initialized with userDataFile: {UserDataFile}", _userDataFile);
		}

		public List<string> GetUserIds()
		{
			var (userIds, _) = LoadUserData();
			if (userIds != null && userIds.Count > 0)
			{
				Log.Information("Loaded userIds: {UserIds}", string.Join(", ", userIds));
				return userIds;
			}
			Log.Information("No userIds found");
			return null;
		}

		public void SaveUserIds(IList<string> userIds)
		{
			try
			{
				var (_, token) = LoadUserData();
				var data = new UserData { UserIds = userIds as List<string> ?? userIds.ToList(), Token = token };
				SaveData(data);
				Log.Information("Saved userIds: {UserIds} to {UserDataFile}", string.Join(", ", userIds), _userDataFile);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error saving user IDs");
				throw;
			}
		}

		public (List<string> userIds, string token) LoadUserData()
		{
			try
			{
				var data = LoadData<UserData>();
				return (data?.UserIds, data?.Token);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading user data");
				return (null, null);
			}
		}

		public void SaveData<T>(T data)
		{
			lock (_lock)
			{
				try
				{
					string json = JsonConvert.SerializeObject(data, Formatting.Indented);
					byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
					File.WriteAllBytes(_userDataFile, encrypted);
					Log.Information("Saved encrypted data to {UserDataFile}", _userDataFile);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error saving data to {UserDataFile}", _userDataFile);
					throw;
				}
			}
		}

		public T LoadData<T>(string userId = null)
		{
			lock (_lock)
			{
				try
				{
					string filePath = userId == null ? _userDataFile : GetFilePath($"user_{userId}");
					if (!File.Exists(filePath))
					{
						Log.Information("{File} not found", filePath);
						return default;
					}

					byte[] encrypted = File.ReadAllBytes(filePath);
					byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
					string json = Encoding.UTF8.GetString(decrypted);
					return JsonConvert.DeserializeObject<T>(json);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error loading data");
					return default;
				}
			}
		}

		public async Task SaveDataAsync<T>(string key, T data)
		{
			string filePath = GetFilePath(key);
			await Task.Run(() =>
			{
				lock (_lock)
				{
					try
					{
						string json = JsonConvert.SerializeObject(data, Formatting.Indented);
						byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
						File.WriteAllBytes(filePath, encrypted);
						Log.Information("Saved encrypted data to {FilePath}", filePath);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Error saving data to {FilePath}", filePath);
						throw;
					}
				}
			});
		}

		public async Task<T> LoadDataAsync<T>(string key)
		{
			string filePath = GetFilePath(key);
			return await Task.Run(() =>
			{
				lock (_lock)
				{
					try
					{
						if (!File.Exists(filePath))
						{
							Log.Information("{File} not found", filePath);
							return default;
						}

						byte[] encrypted = File.ReadAllBytes(filePath);
						byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
						string json = Encoding.UTF8.GetString(decrypted);
						return JsonConvert.DeserializeObject<T>(json);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Error loading data from {FilePath}", filePath);
						return default;
					}
				}
			});
		}

		public async Task<bool> ContainsDataAsync(string key)
		{
			string filePath = GetFilePath(key);
			return await Task.FromResult(File.Exists(filePath));
		}

		public async Task RemoveDataAsync(string key)
		{
			string filePath = GetFilePath(key);
			if (File.Exists(filePath))
				await Task.Run(() => File.Delete(filePath));
		}

		public async Task ClearAllDataAsync()
		{
			string directory = Path.GetDirectoryName(_userDataFile);
			if (Directory.Exists(directory))
			{
				foreach (string file in Directory.GetFiles(directory))
				{
					await Task.Run(() => File.Delete(file));
				}
			}
		}

		public async Task SaveAvatarAsync(string userId, string filePath)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId));
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
				throw new ArgumentException("Invalid avatar file path", nameof(filePath));

			string destinationPath = Path.Combine(
				Path.GetDirectoryName(_userDataFile),
				$"avatar_{userId}{Path.GetExtension(filePath)}"
			);

			try
			{
				await File.WriteAllBytesAsync(destinationPath, File.ReadAllBytes(filePath));
				Log.Information("Saved avatar locally for user {UserId} at {DestinationPath}", userId, destinationPath);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save avatar locally for user {UserId}", userId);
				throw;
			}
		}

		public async Task<string> CreateUserIdAsync(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username cannot be empty", nameof(username));

			try
			{
				// Generate UserId using SHA-256 of username + timestamp
				using var sha256 = SHA256.Create();
				byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(username + DateTime.UtcNow.ToString("o")));
				string userId = Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Substring(0, 16);

				// Load existing user IDs
				var (existingUserIds, token) = LoadUserData();
				existingUserIds ??= new List<string>();

				if (!existingUserIds.Contains(userId))
				{
					existingUserIds.Add(userId);
					SaveUserIds(existingUserIds);
				}

				// Save username associated with userId
				await SaveDataAsync($"profile_{userId}", new { Username = username });

				Log.Information("Created and saved UserId {UserId} for username {Username}", userId, username);
				return userId;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to create UserId for username {Username}", username);
				throw;
			}
		}

		// Методы для кэширования (перенесены из CacheService)

		public async Task CacheDataAsync<T>(string key, T data, TimeSpan expiry)
		{
			var cacheKey = $"cache_{key}";
			var cacheEntry = new CacheEntry<T>
			{
				Data = data,
				Expiry = DateTime.Now + expiry
			};
			await SaveDataAsync(cacheKey, cacheEntry);
			Log.Information("Cached data for key: {Key}", key);
		}

		public async Task<T> GetCachedDataAsync<T>(string key)
		{
			var cacheKey = $"cache_{key}";
			var entry = await LoadDataAsync<CacheEntry<T>>(cacheKey);
			if (entry != null && (entry.Expiry == null || entry.Expiry > DateTime.Now))
			{
				Log.Information("Retrieved cached data for key: {Key}", key);
				return entry.Data;
			}
			await RemoveDataAsync(cacheKey);
			Log.Information("Cache expired or not found for key: {Key}", key);
			return default;
		}

		public async Task ClearCacheAsync(string keyPrefix)
		{
			string directory = Path.GetDirectoryName(_userDataFile);
			if (Directory.Exists(directory))
			{
				foreach (string file in Directory.GetFiles(directory, $"cache_{keyPrefix}*.dat"))
				{
					await Task.Run(() => File.Delete(file));
					Log.Information("Cleared cache file: {File}", file);
				}
			}
			Log.Information("Cleared cache for prefix: {KeyPrefix}", keyPrefix);
		}

		// Методы для локального хранения данных

		public async Task<List<Message>> GetHistoryAsync(string userId, int page = 1, int pageSize = 50)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_{userId}_page{page}";
			var history = await LoadDataAsync<List<Message>>(cacheKey);
			if (history != null)
			{
				Log.Information("Returning history for user {UserId}, page {Page}", userId, page);
				return history.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			}

			Log.Information("No history found for user {UserId}, page {Page}", userId, page);
			return new List<Message>();
		}

		public async Task SaveHistoryAsync(string userId, List<Message> history)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_{userId}_page1";
			await SaveDataAsync(cacheKey, history);
			Log.Information("Saved history for user {UserId}", userId);
		}

		public async Task<Dictionary<string, int>> GetMoodStatsAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"mood_stats_{userId}";
			var stats = await LoadDataAsync<Dictionary<string, int>>(cacheKey);
			if (stats != null)
			{
				Log.Information("Returning mood stats for user {UserId}", userId);
				return stats;
			}

			Log.Information("No mood stats found for user {UserId}", userId);
			return new Dictionary<string, int>();
		}

		public async Task SaveMoodStatsAsync(string userId, Dictionary<string, int> stats)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"mood_stats_{userId}";
			await SaveDataAsync(cacheKey, stats);
			Log.Information("Saved mood stats for user {UserId}", userId);
		}

		public async Task<List<string>> GetChatHistoryListAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_list_{userId}";
			var list = await LoadDataAsync<List<string>>(cacheKey);
			if (list != null)
			{
				Log.Information("Returning chat history list for user {UserId}", userId);
				return list;
			}

			Log.Information("No chat history list found for user {UserId}", userId);
			return new List<string>();
		}

		public async Task SaveChatHistoryListAsync(string userId, List<string> list)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_list_{userId}";
			await SaveDataAsync(cacheKey, list);
			Log.Information("Saved chat history list for user {UserId}", userId);
		}

		public async Task ClearChatHistoryAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_{userId}";
			await RemoveDataAsync(cacheKey);
			Log.Information("Chat history cleared for user {UserId}", userId);
		}

		public async Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId, int page = 1, int pageSize = 50)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"diary_entries_{userId}_page{page}";
			var entries = await LoadDataAsync<List<DiaryEntry>>(cacheKey);
			if (entries != null)
			{
				Log.Information("Returning diary entries for user {UserId}, page {Page}", userId, page);
				return entries.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			}

			Log.Information("No diary entries found for user {UserId}, page {Page}", userId, page);
			return new List<DiaryEntry>();
		}

		public async Task CreateDiaryEntryAsync(string userId, DiaryEntry entry)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (entry == null)
				throw new ArgumentNullException(nameof(entry), "Diary entry cannot be null");

			var cacheKey = $"diary_entries_{userId}_page1";
			var entries = await LoadDataAsync<List<DiaryEntry>>(cacheKey) ?? new List<DiaryEntry>();
			entries.Add(entry);
			await SaveDataAsync(cacheKey, entries);
			Log.Information("Diary entry created for user {UserId}", userId);
		}

		public async Task<List<string>> GetDiaryTagsAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"diary_tags_{userId}";
			var tags = await LoadDataAsync<List<string>>(cacheKey);
			if (tags != null)
			{
				Log.Information("Returning diary tags for user {UserId}", userId);
				return tags;
			}

			Log.Information("No diary tags found for user {UserId}", userId);
			return new List<string>();
		}

		public async Task AddDiaryTagAsync(string userId, string tag)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (string.IsNullOrEmpty(tag))
				throw new ArgumentNullException(nameof(tag), "Tag cannot be empty");

			var cacheKey = $"diary_tags_{userId}";
			var tags = await LoadDataAsync<List<string>>(cacheKey) ?? new List<string>();
			if (!tags.Contains(tag))
			{
				tags.Add(tag);
				await SaveDataAsync(cacheKey, tags);
				Log.Information("Diary tag added for user {UserId}: {Tag}", userId, tag);
			}
		}

		public async Task ArchiveDiaryEntriesAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"diary_entries_{userId}";
			await RemoveDataAsync(cacheKey);
			Log.Information("Diary entries archived for user {UserId}", userId);
		}

		public async Task<List<string>> GetTriggersAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"triggers_{userId}";
			var triggers = await LoadDataAsync<List<string>>(cacheKey);
			if (triggers != null)
			{
				Log.Information("Returning triggers for user {UserId}", userId);
				return triggers;
			}

			Log.Information("No triggers found for user {UserId}", userId);
			return new List<string>();
		}

		public async Task AddTriggerAsync(string userId, string trigger)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (string.IsNullOrEmpty(trigger))
				throw new ArgumentNullException(nameof(trigger), "Trigger cannot be empty");

			var cacheKey = $"triggers_{userId}";
			var triggers = await LoadDataAsync<List<string>>(cacheKey) ?? new List<string>();
			if (!triggers.Contains(trigger))
			{
				triggers.Add(trigger);
				await SaveDataAsync(cacheKey, triggers);
				Log.Information("Trigger added for user {UserId}: {Trigger}", userId, trigger);
			}
		}

		private string GetFilePath(string key)
		{
			string baseDir = Path.GetDirectoryName(_userDataFile);
			string safeKey = key.Replace(Path.DirectorySeparatorChar, '_').Replace('/', '_');
			return Path.Combine(baseDir, $"{safeKey}.dat");
		}
	}
}