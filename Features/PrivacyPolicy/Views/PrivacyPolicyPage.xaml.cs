using ChatBotClient.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows.Controls;

namespace ChatBotClient.Features.PrivacyPolicy.Views
{
	public partial class PrivacyPolicyPage : Page
	{
		public PrivacyPolicyPage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				DataContext = serviceProvider.GetRequiredService<PrivacyPolicyViewModel>();
				Log.Information("PrivacyPolicyPage initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize PrivacyPolicyPage: {Message}", ex.Message);
			}
		}
	}
}