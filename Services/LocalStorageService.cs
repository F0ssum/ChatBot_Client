using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace ChatBotClient.Services
{
	public class LocalStorageService
	{
		private readonly string _storageFile;

		public LocalStorageService()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_storageFile = Path.Combine(appDataPath, "ChatBotClient", "local_storage.json");
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_storageFile));
				Log.Information("LocalStorageService initialized with storage file: {StorageFile}", _storageFile);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize LocalStorageService");
			}
		}

		public List<string> GetUserIds()
		{
			try
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
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading user IDs");
				return null;
			}
		}

		public void SaveUserIds(IList<string> userIds)
		{
			try
			{
				var data = new { userIds, token = (string)null };
				string json = JsonConvert.SerializeObject(data, Formatting.Indented);
				File.WriteAllText(_storageFile, json);
				Log.Information("Saved userIds: {UserIds} to {StorageFile}", string.Join(", ", userIds), _storageFile);
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
				if (!File.Exists(_storageFile))
				{
					Log.Information("{StorageFile} not found", _storageFile);
					return (null, null);
				}

				string json = File.ReadAllText(_storageFile);
				var data = JsonConvert.DeserializeObject<dynamic>(json);
				var userIds = data?.userIds != null ? JsonConvert.DeserializeObject<List<string>>(data.userIds.ToString()) : null;
				var token = data?.token?.ToString();
				Log.Information("Loaded userIds: {UserIds}, token: {Token}", userIds != null ? string.Join(", ", userIds) : "none", token);
				return (userIds, token);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading user data");
				return (null, null);
			}
		}

		public void SaveData<T>(T data)
		{
			try
			{
				string json = JsonConvert.SerializeObject(data, Formatting.Indented);
				File.WriteAllText(_storageFile, json);
				Log.Information("Saved data to {StorageFile}", _storageFile);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error saving data");
				throw;
			}
		}

		public T LoadData<T>()
		{
			try
			{
				if (!File.Exists(_storageFile))
				{
					Log.Information("{StorageFile} not found", _storageFile);
					return default;
				}

				string json = File.ReadAllText(_storageFile);
				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading data");
				return default;
			}
		}
	}
}