using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct TransactionId
	{
		public static implicit operator TransactionId(int transactionId) =>
			new TransactionId(transactionId);
	}

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct TransactionItemId
	{
		public static implicit operator TransactionItemId(int transactionItemId) =>
			new TransactionItemId(transactionItemId);
	}
}
