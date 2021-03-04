using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Management;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Making.Cents.PlaidModule.Views
{
	/// <summary>
	/// Interaction logic for PlaidLinkWindow.xaml
	/// </summary>
	public partial class PlaidLinkWindow : Window
	{
		#region Initialization
		private readonly PlaidClient _plaidClient;
		private readonly PlaidTokens _plaidOptions;
		private readonly ILogger<PlaidLinkWindow> _logger;

		public PlaidLinkWindow(
			PlaidClient plaidClient,
			IOptionsSnapshot<PlaidTokens> plaidOptions,
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

		#region Refresh Token
		public bool RefreshAccessToken(string source)
		{
			_logger.LogInformation("Starting Token refresh for source {source}", source);

			_source = source;
			_mode = Mode.Refresh;
			return this.ShowDialog() ?? false;
		}

		private Task<CreateLinkTokenResponse> GetRefreshToken()
		{
			var token = _plaidOptions.AccessTokens[_source!];
			return _plaidClient.CreateLinkTokenAsync(
				new CreateLinkTokenRequest()
				{
					AccessToken = token,
					User = new User { ClientUserId = _plaidOptions.PlaidUserId, },
					ClientName = "Making.Cents",
					Language = Going.Plaid.Entity.Language.English,
					CountryCodes = new[] { "US" },
				});

		}
		#endregion

		#region Event Handlers
		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_logger.LogTrace("Window loaded; Initializing webview/Plaid Link.");

			var linkToken = await (
				_mode switch
				{
					Mode.New => throw new NotImplementedException(),
					Mode.Refresh => GetRefreshToken(),
					_ => throw new InvalidOperationException("Should never get here."),
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

			EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs> navigationFunc = null!;
			navigationFunc = NavigationCompleted;
			webView.NavigationCompleted += navigationFunc;

			var folder = Path.GetDirectoryName(
				Assembly.GetExecutingAssembly().Location)!;
			var htmlFileLocation = Path.Combine(folder, "Resources", "PlaidLink.html");
			var uri = "file:///" + htmlFileLocation.Replace('\\', '/');
			webView.Source = new Uri(uri);

			async void NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
			{
				webView.NavigationCompleted -= navigationFunc;
				await webView.ExecuteScriptAsync($"RunPlaidLink('{linkToken.LinkToken}');");

				_logger.LogInformation("Link Dialog code complete.");
			}
		}

		private class LinkResponse
		{
			public bool Success { get; set; }
			public string? Token { get; set; }
		}

		private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
		{
			var response = System.Text.Json.JsonSerializer.Deserialize<LinkResponse>(e.WebMessageAsJson)!;
			_logger.LogInformation(
				"Completed link process. Success: {Success}; Token: {Token}.",
				response.Success,
				response.Token);

			if (_mode == Mode.New)
			{
				// update appsettings.secret.json
				throw new NotImplementedException();
			}

			_logger.LogInformation("Closing Dialog.");

			this.DialogResult = response.Success;
			this.Close();
		}
		#endregion
	}
}
