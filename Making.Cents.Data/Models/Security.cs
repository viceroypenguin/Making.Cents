using LinqToDB.Mapping;
using Making.Cents.Common.Ids;
using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Data.Models
{
	[Table(Schema = "dbo", Name = "Security")]
	public class Security
	{
		[PrimaryKey, DataType(LinqToDB.DataType.Int32), Identity] public SecurityId SecurityId { get; set; }

		[Column, NotNull] public string Ticker { get; set; } = null!;
		[Column, NotNull] public string Name { get; set; } = null!;
	}

	[Table(Schema = "dbo", Name = "SecurityValue")]
	public class SecurityValue
	{
		[PrimaryKey(1), NotNull, DataType(LinqToDB.DataType.Int32)] public SecurityId SecurityId { get; set; }
		[PrimaryKey(2)] public DateTime Date { get; set; }

		[Column, NotNull] public decimal Value { get; set; } = 1;
	}

}
