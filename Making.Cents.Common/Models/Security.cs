using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Ids;

namespace Making.Cents.Common.Models
{
	public class Security
	{
		public static SecurityId CashSecurityId { get; } =
			new Guid("ca000000-0000-0000-0000-000000000000");

		public SecurityId SecurityId { get; set; }
		public string Ticker { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;

		public decimal CurrentValue { get; set; }
		public DateTime CurrentValueDate { get; set; }
	}
}
