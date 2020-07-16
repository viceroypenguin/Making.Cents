using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Common;
using Making.Cents.Common.Ids;

namespace Making.Cents.Data.Converters
{
	internal class SecurityIdConverter : ValueConverter<SecurityId, int>
	{
		public SecurityIdConverter()
			: base(
				  v => v.Value,
				  p => (SecurityId)p,
				  handlesNulls: false)
		{ }
	}
}
