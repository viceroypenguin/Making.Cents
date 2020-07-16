using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class AccountTypeIdConverter : ValueConverter<AccountTypeId, int>
	{
		public AccountTypeIdConverter()
			: base(
				  v => v.Value,
				  p => new AccountTypeId(p),
				  handlesNulls: false)
		{ }
	}

	internal class AccountSubTypeIdConverter : ValueConverter<AccountSubTypeId, int>
	{
		public AccountSubTypeIdConverter()
			: base(
				  v => v.Value,
				  p => new AccountSubTypeId(p),
				  handlesNulls: false)
		{ }
	}

	internal class AccountIdConverter : ValueConverter<AccountId, int>
	{
		public AccountIdConverter()
			: base(
				  v => v.Value,
				  p => new AccountId(p),
				  handlesNulls: false)
		{ }
	}
}
