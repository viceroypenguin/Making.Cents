using LinqToDB;
using LinqToDB.Mapping;
using Making.Cents.Common.Ids;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Making.Cents.Data.Models
{
	[Table(Schema = "dbo", Name = "Transaction")]
	public class Transaction
	{
		[PrimaryKey, DataType(LinqToDB.DataType.Int32)] public TransactionId TransactionId { get; set; }
		[Column, NotNull] public DateTime Date { get; set; }
		[Column, NotNull] public string Description { get; set; } = null!;
		[Column, Nullable] public string? Memo { get; set; }
	}

	[Table(Schema = "dbo", Name = "ClearedStatus")]
	public class ClearedStatus
	{
		[PrimaryKey, DataType(LinqToDB.DataType.Int32)] public ClearedStatusId ClearedStatusId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "TransactionItem")]
	public class TransactionItem
	{
		[PrimaryKey(1), DataType(LinqToDB.DataType.Int32)] public TransactionId TransactionId { get; set; }
		[PrimaryKey(2), DataType(LinqToDB.DataType.Int32), Identity] public TransactionItemId TransactionItemId { get; set; }

		[Column, NotNull] public AccountId AccountId { get; set; }
		[Column, NotNull] public StockId StockId { get; set; }
		[Column, NotNull] public decimal Shares { get; set; }
		[Column, NotNull] public decimal Amount { get; set; }
		[ExpressionMethod(nameof(PerShareExpr), IsColumn = true)] public decimal PerShare { get; set; }

		[Column, NotNull] public ClearedStatusId ClearedStatusId { get; set; }
		[Column, Nullable] public string? Memo { get; set; }

		private static Expression<Func<TransactionItem, decimal>> PerShareExpr() =>
			ti => Math.Round(ti.Amount / ti.Shares, 4);
	}
}
