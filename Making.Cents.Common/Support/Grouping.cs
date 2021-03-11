using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Support
{
	public class Grouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
	{
		public Grouping(TKey key) =>
			Key = key;

		public Grouping(TKey key, IEnumerable<TElement> data)
			: base(data) =>
			Key = key;

		public TKey Key { get; }
	}
}
