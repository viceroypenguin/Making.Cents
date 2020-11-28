using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Enums;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
	public partial struct TransactionId
	{
		public static explicit operator Guid(TransactionId transactionId) =>
			transactionId.Value;
		public static implicit operator TransactionId(Guid transactionId) =>
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

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
	public partial struct TransactionItemId
	{
		public static explicit operator Guid(TransactionItemId transactionItemId) =>
			transactionItemId.Value;
		public static implicit operator TransactionItemId(Guid transactionItemId) =>
			new TransactionItemId(transactionItemId);
	}
}
