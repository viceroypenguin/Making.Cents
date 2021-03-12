using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Support
{
	public static class ProjectionComparer
	{
		public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(
			Func<TSource, TKey> projection)
			=> new ProjectionComparer<TSource, TKey>(projection);
	}

	public static class ProjectionComparer<TSource>
	{
		public static ProjectionComparer<TSource, TKey> Create<TKey>(
			Func<TSource, TKey> projection)
			=> new ProjectionComparer<TSource, TKey>(projection);
	}

	public class ProjectionComparer<TSource, TKey> : IComparer<TSource>
	{
		readonly Func<TSource, TKey> projection;
		readonly IComparer<TKey> comparer;

		public ProjectionComparer(
			Func<TSource, TKey> projection)
			: this(projection, null) { }

		public ProjectionComparer(
			Func<TSource, TKey> projection,
			IComparer<TKey>? comparer)
		{
			this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
			this.comparer = comparer ?? Comparer<TKey>.Default;
		}

		public int Compare(TSource? x, TSource? y)
		{
			// Don't want to project from nullity
			if (x == null && y == null)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;
			return comparer.Compare(projection(x), projection(y));
		}
	}
}
