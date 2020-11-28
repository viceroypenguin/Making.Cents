using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Going.Plaid;
using LinqToDB;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Models;
using Newtonsoft.Json;

using PlaidAccountType = Going.Plaid.Entity.AccountType;
using PlaidAccountSubType = Going.Plaid.Entity.AccountSubType;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data.Services
{
	public class AccountService
	{
		#region Initialization
		private readonly Func<DbContext> _context;
		private readonly ILogger<AccountService> _logger;

		public AccountService(
			Func<DbContext> context,
			ILogger<AccountService> logger)
		{
			_context = context;
			_logger = logger;
		}
		#endregion

		#region Query
		private List<Account>? _accounts;

		public async ValueTask<IReadOnlyList<Account>> GetDbAccounts()
		{
			return _accounts ??= await _();

			async Task<List<Account>> _()
			{
				_logger.LogTrace("Downloading accounts from database.");
				using (var c = _context())
				{
					return await c.Accounts
						.Select(a => new Account
						{
							AccountId = a.AccountId,
							Name = a.Name,

							AccountType = a.AccountTypeId,
							AccountSubType = a.AccountSubTypeId,

							PlaidSource = a.PlaidSource,
							PlaidAccountData = string.IsNullOrWhiteSpace(a.PlaidAccountData)
								? null
								: JsonConvert.DeserializeObject<Going.Plaid.Entity.Account>(a.PlaidAccountData),
						})
						.ToListAsync();
				}
			}
		}

		public async Task<IReadOnlyList<Account>> GetAccountBalances(DateTime date)
		{
			using (var c = _context())
				return await (
					from t in c.Transactions
					where t.Date < date
					from ti in c.TransactionItems.Where(ti => ti.TransactionId == t.TransactionId)
					from a in c.Accounts.Where(a => a.AccountId == ti.AccountId)
					from sv in c.SecurityValues
						.Where(sv => sv.SecurityId == ti.SecurityId)
						.Where(sv => sv.Date < date)
						.OrderByDescending(sv => sv.Date)
						.Take(1)
					group ti.Shares * sv.Value by a into x
					select new Account
					{
						AccountId = x.Key.AccountId,
						Name = x.Key.Name,
						AccountType = x.Key.AccountTypeId,
						AccountSubType = x.Key.AccountSubTypeId,

						Balance = x.Sum(),
					})
					.ToListAsync();
		}
		#endregion

		#region Save
		public async Task<Account> AddAccount(Account account)
		{
			Guard.Argument(account.Name, "account.Name").NotWhiteSpace();
			_logger.LogTrace("Saving Account {AccountName} (ID: {AccountId})", account.Name, account.AccountId.Value);

			using (var c = _context())
			{
				var dbAccount = await c.Accounts
					.InsertWithOutputAsync(
						new Data.Models.Account
						{
							AccountId = (Guid)account.AccountId,
							Name = account.Name,

							AccountTypeId = account.AccountType,
							AccountSubTypeId = account.AccountSubType,

							PlaidSource = account.PlaidSource,
							PlaidAccountData = account.PlaidAccountData != null
								? JsonConvert.SerializeObject(account.PlaidAccountData)
								: null,
						});

				account.AccountId = dbAccount.AccountId;
			}

			_accounts?.Add(account);

			_logger.LogInformation("Saved Account {AccountName} (ID: {AccountId})", account.Name, account.AccountId.Value);
			return account;
		}

		public async Task UpdateAccounts(IEnumerable<Account> accounts)
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
		}

		#endregion
	}
}
