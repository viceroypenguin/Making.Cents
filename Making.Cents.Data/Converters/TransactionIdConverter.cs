using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class ClearedStatusIdConverter : ValueConverter<ClearedStatusId, int>
	{
		public ClearedStatusIdConverter()
			: base(
				  v => v.Value,
				  p => new ClearedStatusId(p),
				  handlesNulls: false)
		{ }
	}

	internal class TransactionIdConverter : ValueConverter<TransactionId, Guid>
	{
		public TransactionIdConverter()
			: base(
				  v => v.Value,
				  p => new TransactionId(p),
				  handlesNulls: false)
		{ }
	}

	internal class TransactionItemIdConverter : ValueConverter<TransactionItemId, Guid>
	{
		public TransactionItemIdConverter()
			: base(
				  v => v.Value,
				  p => new TransactionItemId(p),
				  handlesNulls: false)
		{ }
	}
}
