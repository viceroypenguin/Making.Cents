using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
	public partial struct StockId
	{
		public static implicit operator Guid(StockId stockId) =>
			stockId.Value;
		public static implicit operator StockId(Guid stockId) =>
			new StockId(stockId);
	}
}
