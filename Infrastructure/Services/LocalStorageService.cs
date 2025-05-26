using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Interfaces;
using ChatBotClient.Core.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotClient.Infrastructure.Services
{
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
					string filePath = userId == null ? _userDataFile : $"user_{userId}.dat";

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

		// ✅ Добавлено: Реализация асинхронных методов из ILocalStorageService

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

		private string GetFilePath(string key)
		{
			// Например: chat_user123.json → AppData\ChatBotClient\chat_user123.dat
			string baseDir = Path.GetDirectoryName(_userDataFile);
			string safeKey = key.Replace(Path.DirectorySeparatorChar, '_').Replace('/', '_');
			return Path.Combine(baseDir, $"{safeKey}.dat");
		}
	}
}