using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Support;

namespace Making.Cents.Common.Models
{
	public class Transaction
	{
		public TransactionId TransactionId { get; set; } = SequentialGuid.Next();
		public DateTime Date { get; set; }
		public string Description { get; set; } = string.Empty;
		public string? Memo { get; set; }

		public List<TransactionItem> TransactionItems { get; set; } =
			new List<TransactionItem>();

		public decimal Balance => TransactionItems?.Sum(i => i.Amount) ?? 0m;
	}

	public class TransactionItem
	{
		public TransactionItemId TransactionItemId { get; set; }
		public AccountId AccountId { get; set; }
		public SecurityId SecurityId { get; set; }
		public decimal Shares { get; set; }
		public decimal Amount { get; set; }
		public decimal PerShare => Math.Round(Amount / Shares, 4);

		public ClearedStatus ClearedStatus { get; set; }
		public string? Memo { get; set; }
		public string? PlaidTransactionData { get; set; }

		[DisallowNull] public Account? Account { get; set; }
		[DisallowNull] public Security? Security { get; set; }
	}
}
