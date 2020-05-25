using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Data.Models
{
	[Table(Schema = "dbo", Name = "AccountType")]
	public class AccountType
	{
		[PrimaryKey, NotNull] public int AccountTypeId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "AccountSubType")]
	public class AccountSubType
	{
		[PrimaryKey, NotNull] public int AccountSubTypeId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "Account")]
	public class Account
	{
		[PrimaryKey, NotNull] public Guid AccountId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
		[Column, NotNull] public string FullName { get; set; } = null!;

		[Column, NotNull] public int AccountTypeId { get; set; }
		[Column, NotNull] public int AccountSubTypeId { get; set; }

		[Column, Nullable] public Guid? ParentAccountId { get; set; }

		[Column, Nullable] public string? PlaidSource { get; set; }
		[Column, Nullable] public string? PlaidAccountData { get; set; } // json
	}
}
