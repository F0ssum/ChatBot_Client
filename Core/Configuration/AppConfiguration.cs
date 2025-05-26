using System;
using System.Configuration;
using System.IO;

namespace ChatBotClient.Core.Configuration
{
	public class AppConfiguration
	{
		public string AppName => "ChatBotClient";

		public string ApiBaseUrl { get; set; } = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080/";
		public double ApiTimeoutSeconds { get; set; } = double.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out double timeout) ? timeout : 30;
		public string ApiKey { get; set; } = ConfigurationManager.AppSettings["ApiKey"];
		public bool UseLocalModel { get; set; } = bool.TryParse(ConfigurationManager.AppSettings["UseLocalModel"], out bool useLocal) && useLocal;
		public string SpeechServiceKey { get; set; } = ConfigurationManager.AppSettings["SpeechServiceKey"];
		public string SpeechServiceRegion { get; set; } = ConfigurationManager.AppSettings["SpeechServiceRegion"];
		public string LocalModelPath { get; set; } = ConfigurationManager.AppSettings["LocalModelPath"] ?? "Models/model.bin";
		public string DataFolderPath { get; set; } = ConfigurationManager.AppSettings["DataFolderPath"]
			?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

		public string AnalyticsDatabaseConnectionString { get; set; } = ConfigurationManager.AppSettings["AnalyticsDatabaseConnectionString"];

		private readonly string _connectionString = "Data Source=emotionAid.db;Version=3;";

		public AppConfiguration()
		{
			// Если AnalyticsDatabaseConnectionString не задан — используем значение по умолчанию
			if (string.IsNullOrEmpty(AnalyticsDatabaseConnectionString))
			{
				AnalyticsDatabaseConnectionString = _connectionString;
			}
		}
	}
}