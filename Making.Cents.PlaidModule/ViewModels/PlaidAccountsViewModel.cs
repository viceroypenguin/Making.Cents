using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Going.Plaid;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Models;
using Making.Cents.Data.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlaidAccountSubType = Going.Plaid.Entity.AccountSubType;
using PlaidAccountType = Going.Plaid.Entity.AccountType;
using PlaidLinkWindow = Making.Cents.PlaidModule.Views.PlaidLinkWindow;
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.PlaidModule.ViewModels
{
	public class PlaidAccountsViewModel : ViewModelBase
	{
		#region Initialization
		private IMessageBoxService MessageBoxService =>
			GetService<IMessageBoxService>();

		private readonly AccountService _accountService;
		private readonly PlaidClient _plaidClient;
		private readonly IReadOnlyDictionary<string, string> _items;
		private readonly Func<PlaidLinkWindow> _newPlaidLinkWindow;
		private readonly ILogger<PlaidAccountsViewModel> _logger;

		public PlaidAccountsViewModel(
			AccountService accountService,
			PlaidClient plaidClient,
			IOptionsSnapshot<PlaidTokens> plaidOptions,
			Func<PlaidLinkWindow> newPlaidLinkWindow,
			ILogger<PlaidAccountsViewModel> logger)
		{
			_accountService = accountService;
			_plaidClient = plaidClient;
			_items = plaidOptions.Value.AccessTokens;
			_newPlaidLinkWindow = newPlaidLinkWindow;
			_logger = logger;

			PlaidSources = _items.Keys.ToArray();
			SelectedPlaidSource = PlaidSources.FirstOrDefault();
		}
		#endregion

		#region Properties
		public IReadOnlyList<string> PlaidSources { get; }
		public string? SelectedPlaidSource { get; set; }

		private readonly Dictionary<string, IReadOnlyList<AccountViewModel>> _accounts = new();
		public IReadOnlyList<AccountViewModel> Accounts { get; private set; } =
			Array.Empty<AccountViewModel>();

		public LoadingViewModel LoadingViewModel { get; } = new();
		#endregion

		#region Commands
		private void OnSelectedPlaidSourceChanged() =>
			_ = LoadAccounts();

		[AsyncCommand]
		public async Task LoadAccounts()
		{
			Accounts = Array.Empty<AccountViewModel>();
			if (string.IsNullOrWhiteSpace(SelectedPlaidSource))
				return;

			if (!_accounts.ContainsKey(SelectedPlaidSource))
			{
				var accounts = await GetPlaidAccounts(SelectedPlaidSource);
				if (accounts != null)
					_accounts[SelectedPlaidSource] = accounts;
			}

			Accounts = _accounts.GetValueOrDefault(SelectedPlaidSource, Array.Empty<AccountViewModel>());
		}

		[AsyncCommand]
		public async Task AddAccount(AccountViewModel avm)
		{
			await _accountService.Save(avm.Account);
			avm.IsCreatedAccount = true;
		}
		#endregion

		#region Plaid
		private Dictionary<string, AccountViewModel>? _accountsByPlaidId;

		private async Task<IReadOnlyList<AccountViewModel>?> GetPlaidAccounts(string source)
		{
			_logger.LogInformation("Downloading Accounts for plaid source {source}", source);

			_logger.LogTrace("Starting plaid api call.");
			var plaidAccounts = await _plaidClient
				.FetchAccountAsync(
					new Going.Plaid.Balance.GetAccountRequest()
					{
						AccessToken = _items[source],
					});

			if (!plaidAccounts.IsSuccessStatusCode)
			{
				_logger.LogWarning(
					"Error downloading plaid accounts. Type: {type}; Code: {code}",
					plaidAccounts.Exception!.ErrorType,
					plaidAccounts.Exception!.ErrorCode);
				switch (plaidAccounts.Exception.ErrorCode)
				{
					case Going.Plaid.Entity.ErrorCode.ItemLoginRequired:
						if (UpdateLink(source))
							return await GetPlaidAccounts(source);
						return null;

					default:
						MessageBoxService.ShowMessage(
							messageBoxText: $"Unable to download accounts for Source '{source}'",
							caption: "Failure Downloading Accounts",
							MessageButton.OK);
						throw new InvalidOperationException(plaidAccounts.Exception!.ErrorMessage);
				}
			}

			_logger.LogInformation("Downloaded {count} accounts from Plaid.", plaidAccounts.Accounts.Length);
			var map = _accountsByPlaidId
				??= _accountService.GetAccounts()
						.Where(a => a.PlaidAccountData != null)
						.ToDictionary(
							a => a.PlaidAccountData!.AccountId,
							a => new AccountViewModel { Account = a, IsCreatedAccount = true, });

			return plaidAccounts
				.Accounts
				.Select(a =>
					map.GetValueOrDefault(a.AccountId)
					?? new AccountViewModel
					{
						Account = new Account
						{
							Name = a.Name,

							AccountType = GetAccountType(a.Type),
							AccountSubType = GetAccountSubType(a.Type, a.SubType),

							PlaidSource = source,
							PlaidAccountData = a,

							ShowOnMainScreen = true,
						},
						IsCreatedAccount = false,
					})
				.ToArray();
		}

		public class AccountViewModel : ViewModelBase
		{
			public Account Account { get; init; } = null!;
			public bool IsCreatedAccount { get; set; }
		}

		#region Translation
		private static AccountType GetAccountType(PlaidAccountType type) =>
			type switch
			{
				PlaidAccountType.Investment => AccountType.Asset,
				PlaidAccountType.Brokerage => AccountType.Asset,
				PlaidAccountType.Depository => AccountType.Asset,
				PlaidAccountType.Credit => AccountType.Liability,
				PlaidAccountType.Loan => AccountType.Liability,
				PlaidAccountType.Mortgage => AccountType.Liability,
				_ => AccountType.Asset,
			};

		private static AccountSubType GetAccountSubType(PlaidAccountType type, PlaidAccountSubType subType) =>
			(type, subType) switch
			{
				(_, PlaidAccountSubType._401a) => AccountSubType.Retirement,
				(_, PlaidAccountSubType._401k) => AccountSubType.Retirement,
				(_, PlaidAccountSubType._403b) => AccountSubType.Retirement,
				(_, PlaidAccountSubType._457b) => AccountSubType.Retirement,
				(_, PlaidAccountSubType._529) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Brokerage) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.CashIsa) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.EducationSavingsAaccount) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Gic) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.HealthReimbursementArrangement) => AccountSubType.Hsa,
				(_, PlaidAccountSubType.Hsa) => AccountSubType.Hsa,
				(_, PlaidAccountSubType.Isa) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Ira) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Lif) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Lira) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Lrif) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Lrsp) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.NonTaxableBrokerageAccount) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Prif) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Rdsp) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Resp) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Rlif) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Rrif) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Pension) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.ProfitSharingPlan) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Retirement) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Roth) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Roth401k) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Rrsp) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.SepIra) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.SimpleIra) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Sipp) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.StockPlan) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.ThriftSavingsPlan) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Tfsa) => AccountSubType.Retirement,
				(_, PlaidAccountSubType.Trust) => AccountSubType.Brokerage,
				(_, PlaidAccountSubType.Ugma) => AccountSubType.Brokerage,
				(_, PlaidAccountSubType.Utma) => AccountSubType.Brokerage,
				(_, PlaidAccountSubType.VariableAnnuity) => AccountSubType.Retirement,
				(PlaidAccountType.Investment, _) => AccountSubType.Brokerage,

				(PlaidAccountType.Depository, _) => AccountSubType.Checking,
				(PlaidAccountType.Credit, _) => AccountSubType.CreditCard,

				(_, PlaidAccountSubType.Auto) => AccountSubType.Loan,
				(_, PlaidAccountSubType.Commercial) => AccountSubType.Loan,
				(_, PlaidAccountSubType.Construction) => AccountSubType.Loan,
				(_, PlaidAccountSubType.Consumer) => AccountSubType.Loan,
				(_, PlaidAccountSubType.HomeEquity) => AccountSubType.Mortgage,
				(_, PlaidAccountSubType.Loan) => AccountSubType.Loan,
				(_, PlaidAccountSubType.Mortgage) => AccountSubType.Mortgage,
				(_, PlaidAccountSubType.Overdraft) => AccountSubType.Loan,
				(_, PlaidAccountSubType.LineOfCredit) => AccountSubType.Loan,
				(_, PlaidAccountSubType.Student) => AccountSubType.Loan,
				(PlaidAccountType.Loan, _) => AccountSubType.OtherLiability,

				_ => AccountSubType.OtherAsset,
			};
		#endregion

		public bool UpdateLink(string source)
		{
			if (MessageBoxService.ShowMessage(
					messageBoxText: $"The token for Source '{source}' has expired, but can be refreshed. Would you like to refresh the link?",
					caption: "Failed Downloading Accounts",
					MessageButton.YesNo,
					MessageIcon.Error) == MessageResult.No)
				return false;

			var window = _newPlaidLinkWindow();
			return window.RefreshAccessToken(source);
		}
		#endregion
	}
}
