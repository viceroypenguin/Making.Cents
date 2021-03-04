using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Making.Cents.Data.Support;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data.Services
{
	public class TransactionService
	{
		#region Initialization
		private readonly Func<DbContext> _context;
		private readonly AccountService _accountService;
		private readonly SecurityService _securityService;
		private readonly ILogger _logger;

		public TransactionService(
			Func<DbContext> context,
			AccountService accountService,
			SecurityService securityService,
			ILogger logger)
		{
			_context = context;
			_accountService = accountService;
			_securityService = securityService;
			_logger = logger;
		}

		private Dictionary<TransactionId, Transaction> _transactionsById = null!;
		private MutableLookup<AccountId, Transaction> _transactionsByAccountId = null!;

		public async Task InitializeAsync()
		{
			_logger.LogTrace("Downloading transactions from database.");
			using (var c = _context())
			{
				var transactions = await c.Transactions
					.LoadWith(t => t.TransactionItems)
					.AsQueryable()
					.AsAsyncEnumerable()
					.Select(t => new Transaction
					{
						TransactionId = t.TransactionId,
						Date = t.Date,
						Description = t.Description,
						Memo = t.Memo,

						TransactionItems = t.TransactionItems
							.Select(ti => new TransactionItem
							{
								TransactionItemId = ti.TransactionItemId,
								AccountId = ti.AccountId,
								SecurityId = ti.SecurityId,
								Shares = ti.Shares,
								Amount = ti.Amount,
								Memo = ti.Memo,
								ClearedStatus = ti.ClearedStatusId,
								PlaidTransactionData = ti.PlaidTransactionData,

								Account = _accountService.GetAccount(ti.AccountId),
								Security = _securityService.GetSecurityById(ti.SecurityId),
							})
							.ToList(),
					})
					.ToListAsync();

				_transactionsById = transactions.ToDictionary(t => t.TransactionId);
				_transactionsByAccountId = transactions
					.SelectMany(t => t.TransactionItems, (t, ti) => (t, ti.AccountId))
					.ToMutableLookup(x => x.AccountId, x => x.t, t => t.Date);
			}
		}
		#endregion

		public IList<Transaction> GetTransactionsForAccount(AccountId accountId) =>
			_transactionsByAccountId[accountId].Values;
	}
}
