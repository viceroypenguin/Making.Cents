using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Dawn;
using LinqToDB;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data.Services
{
	public class AccountService
	{
		#region Initialization
		private readonly Func<DbContext> _context;
		private readonly ILogger _logger;

		public AccountService(
			Func<DbContext> context,
			ILogger logger)
		{
			_context = context;
			_logger = logger;
		}

		private Dictionary<AccountId, Account> _accounts = null!;

		public async Task InitializeAsync()
		{
			_logger.LogTrace("Downloading accounts from database.");
			using (var c = _context())
			{
				_accounts = await c.Accounts
					.Select(a => new Account
					{
						AccountId = a.AccountId,
						Name = a.Name,

						AccountType = a.AccountTypeId,
						AccountSubType = a.AccountSubTypeId,

						PlaidSource = a.PlaidSource,
						PlaidAccountData = string.IsNullOrWhiteSpace(a.PlaidAccountData)
							? null
							: JsonSerializer.Deserialize<Going.Plaid.Entity.Account>(a.PlaidAccountData, null),

						ShowOnMainScreen = a.ShowOnMainScreen,
					})
					.ToDictionaryAsync(a => a.AccountId);
			}
		}
		#endregion

		#region Query
		public IEnumerable<Account> GetAccounts() =>
			_accounts.Values;

		public Account GetAccount(AccountId accountId) =>
			_accounts[accountId];
		#endregion

		#region Save
		public async Task<Account> Save(Account account)
		{
			Guard.Argument(account.Name, "account.Name").NotWhiteSpace();
			_logger.LogTrace("Saving Account {AccountName} (ID: {AccountId})", account.Name, account.AccountId.Value);

			using (var c = _context())
			{
				await c
					.InsertOrReplaceAsync(
						new Data.Models.Account
						{
							AccountId = account.AccountId,
							Name = account.Name,

							AccountTypeId = account.AccountType,
							AccountSubTypeId = account.AccountSubType,

							PlaidSource = account.PlaidSource,
							PlaidAccountData = account.PlaidAccountData != null
								? JsonSerializer.Serialize(account.PlaidAccountData)
								: null,

							ShowOnMainScreen = account.ShowOnMainScreen,
						});
			}

			_accounts[account.AccountId] = account;

			_logger.LogInformation("Saved Account {AccountName} (ID: {AccountId})", account.Name, account.AccountId.Value);
			return account;
		}

		public async Task UpdateAccounts(IReadOnlyCollection<Account> accounts)
		{
			using (var c = _context())
			{
				var affectedRows = await c.Accounts
					.Merge().Using(accounts)
					.On((dst, src) => dst.AccountId == src.AccountId)
					.UpdateWhenMatched((dst, src) => new Data.Models.Account
					{
						Name = src.Name,
						AccountTypeId = src.AccountType,
						AccountSubTypeId = src.AccountSubType,
					})
					.MergeAsync();

				_logger.LogInformation("Updated accounts. {affectedRows} rows updated.");
			}

			foreach (var a in accounts)
			{
				_accounts[a.AccountId].Name = a.Name;
				_accounts[a.AccountId].AccountType = a.AccountType;
				_accounts[a.AccountId].AccountSubType = a.AccountSubType;
			}
		}

		#endregion
	}
}
