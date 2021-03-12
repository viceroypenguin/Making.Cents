using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using LinqToDB;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Making.Cents.Common.Support;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace Making.Cents.Data.Services
{
	public class AccountRefreshRequiredEventArgs : EventArgs
	{
		public AccountId AccountId { get; init; }
	}

	public class TransactionService
	{
		#region Initialization
		private static readonly IComparer<Transaction> _transactionComparer =
			ProjectionComparer<Transaction>.Create(t => (t.Date, t.TransactionId));

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
		private Dictionary<AccountId, List<Transaction>> _transactionsByAccountId = null!;

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
						TransactionType = t.TransactionTypeId,
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
					.GroupBy(x => x.AccountId)
					.ToDictionary(
						g => g.Key,
						g => g
							.Select(x => x.t)
							.DistinctBy(t => t.TransactionId)
							.OrderBy(t => (t.Date, t.TransactionId))
							.ToList());
			}
		}
		#endregion

		#region Public methods
		public IList<Transaction> GetTransactionsForAccount(AccountId accountId) =>
			_transactionsByAccountId[accountId];

		public event EventHandler<AccountRefreshRequiredEventArgs>? AccountRefreshRequired;

		public async Task SaveTransaction(Transaction transaction)
		{
			Guard.Argument(() => transaction.Balance).Zero();

			_logger.LogTrace("Saving Transaction {TransactionMemo} to db (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
			using (var c = _context())
			using (await c.BeginTransactionAsync())
			{
				await c.InsertOrReplaceAsync(
					new Data.Models.Transaction
					{
						TransactionId = transaction.TransactionId,
						TransactionTypeId = transaction.TransactionType,
						Date = transaction.Date,
						Memo = transaction.Memo,
						Description = transaction.Description,
					});

				await c.TransactionItems
					.Merge().Using(transaction.TransactionItems
						.Select(ti => new Data.Models.TransactionItem
						{
							TransactionItemId = ti.TransactionItemId,
							TransactionId = transaction.TransactionId,
							AccountId = ti.AccountId,
							SecurityId = ti.SecurityId,
							Amount = ti.Amount,
							Shares = ti.Shares,
							Memo = ti.Memo,
							ClearedStatusId = ti.ClearedStatus,
							PlaidTransactionData = ti.PlaidTransactionData,
						}))
					.On((dst, src) => dst.TransactionItemId == src.TransactionItemId)
					.InsertWhenNotMatched()
					.UpdateWhenMatched()
					.DeleteWhenNotMatchedBySourceAnd(
						ti => ti.TransactionId == transaction.TransactionId)
					.MergeAsync();

				await c.CommitTransactionAsync();
			}
			_logger.LogTrace("Saved Transaction {TransactionMemo} to db (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);

			var oldTransaction = _transactionsById.GetOrDefault(transaction.TransactionId);
			if (oldTransaction != null)
			{
				if (oldTransaction.Date != transaction.Date)
				{
					RemoveTransactionFromCache(oldTransaction);
					AddTransactionToCache(transaction);
				}
				else
				{
					transaction.Map().To(oldTransaction);
					throw new NotImplementedException("TODO!");
				}
			}
			else
			{
				AddTransactionToCache(transaction);
			}

			_logger.LogDebug("Saved Transaction {TransactionMemo} (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
		}

		public async Task DeleteTransaction(TransactionId transactionId)
		{
			var transaction = _transactionsById[transactionId];

			_logger.LogTrace("Deleting Transaction {TransactionMemo} from db (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
			using (var c = _context())
			using (await c.BeginTransactionAsync())
			{
				await c.InsertOrReplaceAsync(
					new Data.Models.Transaction
					{
						TransactionId = transaction.TransactionId,
						TransactionTypeId = transaction.TransactionType,
						Date = transaction.Date,
						Memo = transaction.Memo,
						Description = transaction.Description,
					});

				await c.TransactionItems
					.Where(ti => ti.TransactionId == transactionId)
					.DeleteAsync();

				await c.Transactions
					.Where(t => t.TransactionId == transactionId)
					.DeleteAsync();

				await c.CommitTransactionAsync();
			}
			_logger.LogTrace("Deleted Transaction {TransactionMemo} from db (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);

			RemoveTransactionFromCache(transaction);
			_logger.LogDebug("Deleted Transaction {TransactionMemo} (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
		}
		#endregion

		#region Private/Support methods
		private void AddTransactionToCache(Transaction transaction)
		{
			_transactionsById[transaction.TransactionId] = transaction;

			foreach (var ti in transaction.TransactionItems
				.DistinctBy(ti => ti.AccountId))
			{
				var list = _transactionsByAccountId.GetOrAdd(ti.AccountId, _ => new List<Transaction>());
				var idx = list.BinarySearch(
					transaction,
					_transactionComparer);
				if (idx >= 0)
					throw new InvalidOperationException(
						"Transaction should not have been part of cache, but was present. "
						+ $"(TransactionId: {transaction.TransactionId};"
						+ $" Memo: {transaction.Memo})");
				list.Insert(~idx, transaction);

				AccountRefreshRequired?.Invoke(
					this,
					new AccountRefreshRequiredEventArgs
					{
						AccountId = ti.AccountId,
					});
			}
			_logger.LogTrace("Added Transaction {TransactionMemo} to cache (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
		}

		private void RemoveTransactionFromCache(Transaction transaction)
		{
			_transactionsById.Remove(transaction.TransactionId);

			foreach (var ti in transaction.TransactionItems
				.DistinctBy(ti => ti.AccountId))
			{
				var list = _transactionsByAccountId.GetOrAdd(ti.AccountId, _ => new List<Transaction>());
				var idx = list.BinarySearch(
					transaction,
					_transactionComparer);
				if (idx < 0)
					throw new InvalidOperationException(
						"Transaction should have been part of cache, but is missing. "
						+ $"(TransactionId: {transaction.TransactionId};"
						+ $" Memo: {transaction.Memo})");
				list.RemoveAt(idx);

				AccountRefreshRequired?.Invoke(
					this,
					new AccountRefreshRequiredEventArgs
					{
						AccountId = ti.AccountId,
					});
			}
			_logger.LogTrace("Removed Transaction {TransactionMemo} from cache (ID: {TransactionId})", transaction.Memo, transaction.TransactionId.Value);
		}
		#endregion
	}
}
