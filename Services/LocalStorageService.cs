using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ChatBotClient.Services
{
	public class LocalStorageService
	{
		private const string StorageFile = "local_storage.json";

		public List<string> GetUserIds()
		{
			var (userIds, _) = LoadUserData();
			if (userIds != null && userIds.Count > 0)
			{
				Console.WriteLine($"Loaded userIds: {string.Join(", ", userIds)}");
				return userIds;
			}

			Console.WriteLine("No userIds found");
			return null;
		}

		public void SaveUserIds(IList<string> userIds)
		{
			var data = new { userIds, token = (string)null };
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(StorageFile, json);
			Console.WriteLine($"Saved userIds: {string.Join(", ", userIds)} to {StorageFile}");
		}

		public (List<string> userIds, string token) LoadUserData()
		{
			if (!File.Exists(StorageFile))
			{
				Console.WriteLine($"{StorageFile} not found");
				return (null, null);
			}

			string json = File.ReadAllText(StorageFile);
			var data = JsonConvert.DeserializeObject<dynamic>(json);
			var userIds = data?.userIds != null ? JsonConvert.DeserializeObject<List<string>>(data.userIds.ToString()) : null;
			var token = data?.token?.ToString();
			Console.WriteLine($"Loaded userIds: {(userIds != null ? string.Join(", ", userIds) : "none")}, token: {token}");
			return (userIds, token);
		}

		public void SaveData<T>(T data)
		{
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(StorageFile, json);
		}

		public T LoadData<T>()
		{
			if (!File.Exists(StorageFile))
			{
				return default(T);
			}

			string json = File.ReadAllText(StorageFile);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}