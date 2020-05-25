using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace Making.Cents.Data.Models
{
	[Table(Schema = "dbo", Name = "VersionHistory")]
	public class VersionHistory
	{
		[PrimaryKey, Identity] public int VersionHistoryId { get; set; }
		[Column, NotNull] public string SqlFile { get; set; } = null!;
		[Column, NotNull] public DateTimeOffset Timestamp { get; set; }
	}
}
