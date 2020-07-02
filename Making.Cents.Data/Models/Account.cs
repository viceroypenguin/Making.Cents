using LinqToDB.Mapping;
using Making.Cents.Common.Ids;
using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Data.Models
{
	[Table(Schema = "dbo", Name = "AccountType")]
	public class AccountType
	{
		[PrimaryKey, NotNull, DataType(LinqToDB.DataType.Int32)] public AccountTypeId AccountTypeId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "AccountSubType")]
	public class AccountSubType
	{
		[PrimaryKey, NotNull, DataType(LinqToDB.DataType.Int32)] public AccountSubTypeId AccountSubTypeId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "Account")]
	public class Account
	{
		[PrimaryKey, NotNull, DataType(LinqToDB.DataType.Int32)] public AccountId AccountId { get; set; }
		[Column, NotNull] public string Name { get; set; } = null!;

		[Column, NotNull, DataType(LinqToDB.DataType.Int32)] public AccountTypeId AccountTypeId { get; set; }
		[Column, NotNull, DataType(LinqToDB.DataType.Int32)] public AccountSubTypeId AccountSubTypeId { get; set; }

		[Column, Nullable] public string? PlaidSource { get; set; }
		[Column, Nullable] public string? PlaidAccountData { get; set; } // json
	}
}
