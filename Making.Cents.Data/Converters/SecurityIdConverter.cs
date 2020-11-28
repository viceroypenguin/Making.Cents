using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class SecurityIdConverter : ValueConverter<SecurityId, Guid>
	{
		public SecurityIdConverter()
			: base(
				  v => v.Value,
				  p => new SecurityId(p),
				  handlesNulls: false)
		{ }
	}
}
