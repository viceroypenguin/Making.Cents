using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Mvvm;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Management;
using Making.Cents.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlaidOptions = Making.Cents.PlaidModule.Models.PlaidOptions;

namespace Making.Cents.PlaidModule.Views
{
	/// <summary>
	/// Interaction logic for PlaidLinkWindow.xaml
	/// </summary>
	public partial class PlaidLinkWindow : Window
	{
		#region Initialization
		private readonly PlaidClient _plaidClient;
		private readonly PlaidOptions _plaidOptions;
		private readonly ILogger<PlaidLinkWindow> _logger;

		public PlaidLinkWindow(
			PlaidClient plaidClient,
			IOptionsSnapshot<PlaidOptions> plaidOptions,
			ILogger<PlaidLinkWindow> logger)
		{
			_plaidClient = plaidClient;
			_plaidOptions = plaidOptions.Value;
			_logger = logger;

			InitializeComponent();
		}

		private enum Mode { New, Refresh, }
		private string? _source;
		private Mode _mode = Mode.New;
		#endregion

		public bool RefreshAccessToken(string source)
		{
			_logger.LogInformation("Starting Token refresh for source {source}", source);

			_source = source;
			_mode = Mode.Refresh;
			return this.ShowDialog() ?? false;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_logger.LogTrace("Window loaded; Initializing webview/Plaid Link.");

			switch (_mode)
			{
				case Mode.New:
					throw new NotImplementedException();

				case Mode.Refresh:
					_ = OnLoadForRefresh();
					break;
			}
		}

		private async Task OnLoadForRefresh()
		{
			var token = _plaidOptions.AccessTokens[_source!];
			var linkToken = await _plaidClient.CreateLinkTokenAsync(
				new CreateLinkTokenRequest()
				{
					AccessToken = token,
					User = new User { ClientUserId = _plaidOptions.PlaidUserId, },
					ClientName = "Making.Cents",
					Language = Going.Plaid.Entity.Language.English,
					CountryCodes = new[] { "US" },
				});

			if (!linkToken.IsSuccessStatusCode)
			{
				_logger.LogWarning(
					"Error getting link token. Type: {type}; Code: {code}",
					linkToken.Exception!.ErrorType,
					linkToken.Exception!.ErrorCode);

				MessageBoxService.ShowMessage(
					messageBoxText: "Failure communicating with Plaid",
					caption: "Plaid Failure",
					MessageButton.OK);
				this.DialogResult = false;
				this.Close();
			}

			_logger.LogInformation("Link Token obtained; showing link dialog.");

			await webView.EnsureCoreWebView2Async();
			var html = await Assembly.GetExecutingAssembly()
				.GetEmbeddedResourceAsync("Making.Cents.PlaidModule.Resources.PlaidLink.html");
			webView.NavigateToString(html);
		}
	}
}
