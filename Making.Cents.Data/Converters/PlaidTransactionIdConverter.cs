using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class PlaidTransactionIdConverter : ValueConverter<PlaidTransactionId, string>
	{
		public PlaidTransactionIdConverter()
			: base(
				  v => v.Value,
				  p => new PlaidTransactionId(p),
				  handlesNulls: false)
		{ }
	}
}
