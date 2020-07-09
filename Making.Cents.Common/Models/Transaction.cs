using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Ids;

namespace Making.Cents.Common.Models
{
	public class Transaction
	{
		public TransactionId TransactionId { get; set; }
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
		public SecurityId StockId { get; set; }
		public decimal Shares { get; set; }
		public decimal Amount { get; set; }
		public decimal PerShare => Amount / Shares;

		public ClearedStatus ClearedStatus { get; set; }
		public string? Memo { get; set; }

		[DisallowNull] public Account? Account { get; set; }
		[DisallowNull] public Security? Stock { get; set; }
	}
}
