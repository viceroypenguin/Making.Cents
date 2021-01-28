using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Enums;
using WrapperValueObject;

namespace Making.Cents.Common.Ids
{
	[WrapperValueObject(typeof(int))]
	public partial struct AccountTypeId
	{
		public static implicit operator AccountType(AccountTypeId typeId) =>
			(AccountType)typeId.Value;
		public static implicit operator AccountTypeId(AccountType type) =>
			new AccountTypeId((int)type);
	}

	[WrapperValueObject(typeof(int))]
	public partial struct AccountSubTypeId
	{
		public static implicit operator AccountSubType(AccountSubTypeId subTypeId) =>
			(AccountSubType)subTypeId.Value;
		public static implicit operator AccountSubTypeId(AccountSubType subType) =>
			new AccountSubTypeId((int)subType);
	}

	[WrapperValueObject]
	public partial struct AccountId { }
}
