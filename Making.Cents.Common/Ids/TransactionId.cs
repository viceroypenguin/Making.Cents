using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Enums;
using WrapperValueObject;

namespace Making.Cents.Common.Ids
{
	[WrapperValueObject(typeof(int))]
	public partial struct ClearedStatusId
	{
		public static implicit operator ClearedStatus(ClearedStatusId clearedStatus) =>
			(ClearedStatus)clearedStatus.Value;
		public static implicit operator ClearedStatusId(ClearedStatus clearedStatus) =>
			new ClearedStatusId((int)clearedStatus);
	}

	[WrapperValueObject(typeof(int))]
	public partial struct TransactionTypeId
	{
		public static implicit operator TransactionType(TransactionTypeId transactionType) =>
			(TransactionType)transactionType.Value;
		public static implicit operator TransactionTypeId(TransactionType transactionType) =>
			new TransactionTypeId((int)transactionType);
	}

	[WrapperValueObject]
	public partial struct TransactionId { }

	[WrapperValueObject]
	public partial struct TransactionItemId { }
}
