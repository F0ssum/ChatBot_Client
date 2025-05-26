using ChatBotClient.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows.Controls;

namespace ChatBotClient.Features.Tree.Views
{
	public partial class TreePage : Page
	{
		public TreePage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				DataContext = serviceProvider.GetRequiredService<TreeViewModel>();
				Log.Information("TreePage initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize TreePage: {Message}", ex.Message);
			}
		}
	}
}