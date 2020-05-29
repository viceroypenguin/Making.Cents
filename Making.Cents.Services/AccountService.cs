using Acklann.Plaid;
using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Data;
using System.Linq;
using LinqToDB;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Models;
using Making.Cents.Common.Support;

namespace Making.Cents.Services
{
	public class AccountService
	{
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

		private List<Account>? _accounts;
		private Dictionary<string, Account>? _accountsByPlaidId;

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
							FullName = a.FullName,

							AccountType = a.AccountTypeId,
							AccountSubType = a.AccountSubTypeId,

							ParentAccountId = a.ParentAccountId,

							PlaidSource = a.PlaidSource,
							PlaidAccountData = string.IsNullOrWhiteSpace(a.PlaidAccountData)
								? null
								: JsonConvert.DeserializeObject<Acklann.Plaid.Entity.Account>(a.PlaidAccountData),
						})
						.ToListAsync();
				}
			}
		}

		public ValueTask<IEnumerable<string>> GetPlaidSources() =>
			new ValueTask<IEnumerable<string>>(_plaidClients.Keys);

		public async Task<IReadOnlyList<Account>> GetPlaidAccounts(string source)
		{
			var dbAccounts = await GetDbAccounts();
			var plaidAccounts = await _plaidClients[source]
				.FetchAccountAsync(
					new Acklann.Plaid.Balance.GetAccountRequest());

			var map = _accountsByPlaidId
				??= dbAccounts
					.Where(a => a.PlaidAccountData != null)
					.ToDictionary(a => a.PlaidAccountData!.Id);

			return plaidAccounts
				.Accounts
				.Select(a =>
					map.GetValueOrDefault(a.Id)
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

		private static AccountType GetAccountType(string type) =>
			type switch
			{
				"investment" => AccountType.Asset,
				"depository" => AccountType.Asset,
				"credit" => AccountType.Liability,
				"loan" => AccountType.Liability,
				_ => AccountType.Asset,
			};

		private static AccountSubType GetAccountSubType(string type, string subType) =>
			(type, subType) switch
			{
				(_, "401a") => AccountSubType.Retirement,
				(_, "401k") => AccountSubType.Retirement,
				(_, "403b") => AccountSubType.Retirement,
				(_, "457b") => AccountSubType.Retirement,
				(_, "529") => AccountSubType.Retirement,
				(_, "brokerage") => AccountSubType.Retirement,
				(_, "cash isa") => AccountSubType.Retirement,
				(_, "education savings account") => AccountSubType.Retirement,
				(_, "gic") => AccountSubType.Retirement,
				(_, "health reimbursement arrangement") => AccountSubType.Hsa,
				(_, "hsa") => AccountSubType.Hsa,
				(_, "isa") => AccountSubType.Retirement,
				(_, "ira") => AccountSubType.Retirement,
				(_, "lif") => AccountSubType.Retirement,
				(_, "lira") => AccountSubType.Retirement,
				(_, "lrif") => AccountSubType.Retirement,
				(_, "lrsp") => AccountSubType.Retirement,
				(_, "non-taxable brokerage account") => AccountSubType.Retirement,
				(_, "prif") => AccountSubType.Retirement,
				(_, "rdsp") => AccountSubType.Retirement,
				(_, "resp") => AccountSubType.Retirement,
				(_, "rlif") => AccountSubType.Retirement,
				(_, "rrif") => AccountSubType.Retirement,
				(_, "pension") => AccountSubType.Retirement,
				(_, "profit sharing plan") => AccountSubType.Retirement,
				(_, "retirement") => AccountSubType.Retirement,
				(_, "roth") => AccountSubType.Retirement,
				(_, "roth 401k") => AccountSubType.Retirement,
				(_, "rrsp") => AccountSubType.Retirement,
				(_, "sep ira") => AccountSubType.Retirement,
				(_, "simple ira") => AccountSubType.Retirement,
				(_, "sipp") => AccountSubType.Retirement,
				(_, "stock plan") => AccountSubType.Retirement,
				(_, "thrift savings plan") => AccountSubType.Retirement,
				(_, "tfsa") => AccountSubType.Retirement,
				(_, "trust") => AccountSubType.Brokerage,
				(_, "ugma") => AccountSubType.Brokerage,
				(_, "utma") => AccountSubType.Brokerage,
				(_, "variable annuity") => AccountSubType.Retirement,
				("investment", _) => AccountSubType.Brokerage,

				("depository", _) => AccountSubType.Checking,
				("credit", _) => AccountSubType.CreditCard,

				(_, "auto") => AccountSubType.Loan,
				(_, "commercial") => AccountSubType.Loan,
				(_, "construction") => AccountSubType.Loan,
				(_, "consumer") => AccountSubType.Loan,
				(_, "home") => AccountSubType.Mortgage,
				(_, "home equity") => AccountSubType.Mortgage,
				(_, "loan") => AccountSubType.Loan,
				(_, "mortgage") => AccountSubType.Mortgage,
				(_, "overdraft") => AccountSubType.Loan,
				(_, "line of credit") => AccountSubType.Loan,
				(_, "student") => AccountSubType.Loan,
				("loan", _) => AccountSubType.OtherLiability,

				_ => AccountSubType.OtherAsset,
			};

		public async Task AddAccount(Common.Models.Account account)
		{
			if (string.IsNullOrWhiteSpace(account.Name))
				throw new ArgumentNullException("account.Name");

			if (string.IsNullOrWhiteSpace(account.FullName))
				throw new ArgumentNullException("account.FullName");

			account.AccountId = SequentialGuid.Next();

			using (var c = _context())
			{
				_ = await c
					.InsertAsync(
						new Data.Models.Account
						{
							AccountId = account.AccountId,
							Name = account.Name,
							FullName = account.FullName,

							AccountTypeId = account.AccountType,
							AccountSubTypeId = account.AccountSubType,

							ParentAccountId = account.ParentAccountId,

							PlaidSource = account.PlaidSource,
							PlaidAccountData = account.PlaidAccountData != null
								? JsonConvert.SerializeObject(account.PlaidAccountData)
								: null,
						});
			}

			_accounts?.Add(account);
		}
	}
}
