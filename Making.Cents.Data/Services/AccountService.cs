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

namespace Making.Cents.Data.Services
{
	public class AccountService
	{
		#region Initialization
		private readonly Func<DbContext> _context;
		private readonly Dictionary<string, PlaidClient> _plaidClients;

		public AccountService(
			Func<DbContext> context,
			IEnumerable<KeyValuePair<string, PlaidClient>> plaidClients)
		{
			_context = context;
			_plaidClients = plaidClients
				.ToDictionary(
					x => x.Key,
					x => x.Value);
		}
		#endregion

		#region Query
		private List<Account>? _accounts;

		public async ValueTask<IReadOnlyList<Account>> GetDbAccounts()
		{
			return _accounts ??= await _();

			async Task<List<Account>> _()
			{
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

		#region Plaid
		private Dictionary<string, Account>? _accountsByPlaidId;

		public IEnumerable<string> GetPlaidSources() => _plaidClients.Keys;

		public async Task<IReadOnlyList<Account>> GetPlaidAccounts(string source)
		{
			var dbAccounts = await GetDbAccounts();
			var plaidAccounts = await _plaidClients[source]
				.FetchAccountAsync(
					new Going.Plaid.Balance.GetAccountRequest());

			if (!plaidAccounts.IsSuccessStatusCode)
				throw plaidAccounts.Exception!;

			var map = _accountsByPlaidId
				??= dbAccounts
					.Where(a => a.PlaidAccountData != null)
					.ToDictionary(a => a.PlaidAccountData!.AccountId);

			return plaidAccounts
				.Accounts
				.Select(a =>
					map.GetValueOrDefault(a.AccountId)
					?? new Account
					{
						Name = a.Name,

						AccountType = GetAccountType(a.Type),
						AccountSubType = GetAccountSubType(a.Type, a.SubType),

						PlaidSource = source,
						PlaidAccountData = a,
					})
				.ToArray();
		}

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

		#region Save
		public async Task<Account> AddAccount(Account account)
		{
			Guard.Argument(account.Name, "account.Name").NotWhiteSpace();

			using (var c = _context())
			{
				var dbAccount = await c.Accounts
					.InsertWithOutputAsync(
						new Data.Models.Account
						{
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
			_accountsByPlaidId = null;

			return account;
		}

		public async Task UpdateAccounts(IEnumerable<Account> accounts)
		{
			using (var c = _context())
			{
				await c.Accounts
					.Merge().Using(accounts)
					.On((dst, src) => dst.AccountId == src.AccountId)
					.UpdateWhenMatched((dst, src) => new Data.Models.Account
					{
						Name = src.Name,
						AccountTypeId = src.AccountType,
						AccountSubTypeId = src.AccountSubType,
					})
					.MergeAsync();
			}
		}

		#endregion
	}
}
