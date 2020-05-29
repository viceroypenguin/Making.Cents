using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Enums;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct AccountTypeId
	{
		public static implicit operator AccountType(AccountTypeId typeId) =>
			(AccountType)typeId.Value;
		public static implicit operator AccountTypeId(AccountType type) =>
			new AccountTypeId((int)type);
	}

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct AccountSubTypeId
	{
		public static implicit operator AccountSubType(AccountSubTypeId subTypeId) =>
			(AccountSubType)subTypeId.Value;
		public static implicit operator AccountSubTypeId(AccountSubType subType) =>
			new AccountSubTypeId((int)subType);
	}

	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
	public partial struct AccountId
	{
		public static implicit operator Guid(AccountId accountId) =>
			accountId.Value;
		public static implicit operator AccountId(Guid accountId) =>
			new AccountId(accountId);
	}
}
