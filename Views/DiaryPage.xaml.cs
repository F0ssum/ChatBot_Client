using ChatBotClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.Views
{
	public partial class DiaryPage : Page
	{
		private readonly DiaryViewModel _viewModel;

		public DiaryPage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				_viewModel = serviceProvider.GetRequiredService<DiaryViewModel>();
				DataContext = _viewModel;
				Log.Information("DiaryPage initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize DiaryPage: {Message}", ex.Message);
				MessageBox.Show($"Failed to initialize diary: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}