using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Extensions
{
	public static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : (dict[key] = value);

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : (dict[key] = value(key));

		public static async Task<TValue> GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, Task<TValue>> value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : (dict[key] = await value(key));

		public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : default;

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : value;

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : value(key);

		public static async Task<TValue> GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, Task<TValue>> value)
			where TKey : notnull => dict.TryGetValue(key, out var v) ? v : await value(key);

		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
			where TKey : notnull
		{
			key = kvp.Key;
			value = kvp.Value;
		}
	}
}
