using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Making.Cents.Services.Models
{
	public class Transaction
	{
		public int TransactionId { get; set; }
		public DateTime Date { get; set; }
		[DisallowNull] public string? Description { get; set; }
		public string? Memo { get; set; }

		public List<TransactionItem> TransactionItems { get; set; } =
			new List<TransactionItem>();

		public decimal Balance => TransactionItems?.Sum(i => i.Amount) ?? 0m;
	}

	public class TransactionItem
	{
		public int TransactionItemId { get; set; }
		public decimal Amount { get; set; }
		public string? Memo { get; set; }

		[DisallowNull] public Account? Account { get; set; }
	}
}
