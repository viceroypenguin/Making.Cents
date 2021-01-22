using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Making.Cents.Common.Extensions;

namespace Making.Cents.Data.Support
{
	internal static class EnumerableExtensions
	{
		public static MutableLookup<TKey, TElement> ToMutableLookup<TSource, TKey, TElement>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector,
			Func<TElement, DateTime> dateSelector)
			where TKey : notnull
		{
			var dict = new Dictionary<TKey, SortedList<DateTime, TElement>>();
			foreach (var i in source.OrderBy(keySelector).ThenBy(i => dateSelector(elementSelector(i))))
			{
				var element = elementSelector(i);
				dict.GetOrAdd(keySelector(i), _ => new())
					.Add(dateSelector(element), element);
			}

			return new MutableLookup<TKey, TElement>(dict);
		}
	}

	internal class MutableLookup<TKey, TElement> where TKey : notnull
	{
		private readonly Dictionary<TKey, SortedList<DateTime, TElement>> _data;

		internal MutableLookup(Dictionary<TKey, SortedList<DateTime, TElement>> dict)
		{
			_data = dict;
		}

		public SortedList<DateTime, TElement> this[TKey key] =>
			_data.GetOrAdd(key, _ => new());

		public int Count => _data.Count;

		public bool Contains(TKey key) => _data.ContainsKey(key);

		public IEnumerable<TKey> Keys => _data.Keys;
	}
}
