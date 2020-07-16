using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class VersionHistoryIdConverter : ValueConverter<VersionHistoryId, int>
	{
		public VersionHistoryIdConverter()
			: base(
				  v => v.Value,
				  p => new VersionHistoryId(p),
				  handlesNulls: false)
		{ }
	}
}
