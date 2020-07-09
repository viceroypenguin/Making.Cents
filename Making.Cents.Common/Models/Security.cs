using System;
using System.Collections.Generic;
using System.Text;
using Making.Cents.Common.Ids;

namespace Making.Cents.Common.Models
{
	public class Security
	{
		public SecurityId StockId { get; set; }
		public string Ticker { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;

		public decimal CurrentValue { get; set; }
		public DateTime CurrentValueDate { get; set; }
	}
}
