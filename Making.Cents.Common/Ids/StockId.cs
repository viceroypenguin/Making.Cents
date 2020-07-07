using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct StockId
	{
		public static implicit operator int(StockId stockId) =>
			stockId.Value;
		public static explicit operator StockId(int stockId) =>
			new StockId(stockId);
	}
}
