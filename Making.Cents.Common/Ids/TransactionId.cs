using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Enums;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct TransactionId
	{
		public static implicit operator int(TransactionId transactionId) =>
			transactionId.Value;
		public static explicit operator TransactionId(int transactionId) =>
			new TransactionId(transactionId);
	}

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct ClearedStatusId
	{
		public static implicit operator ClearedStatus(ClearedStatusId clearedStatus) =>
			(ClearedStatus)clearedStatus.Value;
		public static implicit operator ClearedStatusId(ClearedStatus clearedStatus) =>
			new ClearedStatusId((int)clearedStatus);
	}

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct TransactionItemId
	{
		public static implicit operator int(TransactionItemId transactionItemId) =>
			transactionItemId.Value;
		public static explicit operator TransactionItemId(int transactionItemId) =>
			new TransactionItemId(transactionItemId);
	}
}
