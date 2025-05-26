using ChatBotClient.Core;
using ChatBotClient.Core.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ChatBotClient.Infrastructure.Services
{
	public partial class CacheService : ICacheService
	{
		private readonly object _lock = new();
		private readonly string _cacheFilePath;

		public CacheService(AppConfiguration config)
		{
			if (config == null)
				throw new ArgumentNullException(nameof(config));

			// Убедись, что AppConfiguration содержит свойство DataFolderPath
			var folderPath = config.DataFolderPath;
			_cacheFilePath = Path.Combine(folderPath, "cache.json");
			Directory.CreateDirectory(folderPath);
		}

		public void ClearCache(string keyPrefix)
		{
			lock (_lock)
			{
				var cache = LoadCache();
				if (cache != null)
				{
					var keysToRemove = cache.Keys.Where(k => k.StartsWith(keyPrefix)).ToList();
					foreach (var key in keysToRemove)
					{
						cache.Remove(key);
					}
					SaveCache(cache);
					Log.Information("Cleared cache for prefix: {KeyPrefix}", keyPrefix);
				}
			}
		}

		public void CacheData<T>(string key, T data, TimeSpan expiry)
		{
			lock (_lock)
			{
				try
				{
					var cacheEntry = new CacheEntry<T>
					{
						Data = data,
						Expiry = DateTime.Now + expiry  // Теперь точно не null
					};

					var cache = LoadCache() ?? new Dictionary<string, object>();
					cache[key] = cacheEntry;
					SaveCache(cache);
					Log.Information("Cached data for key: {Key}", key);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error caching data for key: {Key}", key);
					throw;
				}
			}
		}

		public T GetCachedData<T>(string key)
		{
			lock (_lock)
			{
				try
				{
					var cache = LoadCache();
					if (cache != null && cache.TryGetValue(key, out var cacheEntry))
					{
						var entry = JsonConvert.DeserializeObject<CacheEntry<T>>(cacheEntry.ToString());
						if (entry.Expiry == null || entry.Expiry > DateTime.Now)
						{
							Log.Information("Retrieved cached data for key: {Key}", key);
							return entry.Data;
						}
						Log.Information("Cache expired for key: {Key}", key);
						cache.Remove(key);
						SaveCache(cache);
					}
					return default;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error retrieving cached data for key: {Key}", key);
					return default;
				}
			}
		}

		private Dictionary<string, object> LoadCache()
		{
			if (!File.Exists(_cacheFilePath))
				return null;

			string json = File.ReadAllText(_cacheFilePath);
			return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
		}

		private void SaveCache(Dictionary<string, object> cache)
		{
			string json = JsonConvert.SerializeObject(cache, Formatting.Indented);
			File.WriteAllText(_cacheFilePath, json);
		}

	}
}