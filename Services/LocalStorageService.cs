using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ChatBotClient.Services
{
	public class LocalStorageService
	{
		private const string StorageFile = "local_storage.json";

		public void SaveData<T>(T data)
		{
			string json = JsonConvert.SerializeObject(data);
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