using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Making.Cents.Common.Support;

namespace Making.Cents.Common.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<(T? left, U? right)> FullOuterJoin<T, U, K>(
			this IEnumerable<T> left,
			IEnumerable<U> right,
			Func<T, K> leftKeySelector,
			Func<U, K> rightKeySelector) 
			where K : IComparable<K>
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			if (right == null)
				throw new ArgumentNullException(nameof(right));

			if (leftKeySelector == null)
				throw new ArgumentNullException(nameof(leftKeySelector));

			if (rightKeySelector == null)
				throw new ArgumentNullException(nameof(rightKeySelector));

			return FullOuterJoinIterator(left, right, leftKeySelector, rightKeySelector);
		}

		private static IEnumerable<(T? left, U? right)> FullOuterJoinIterator<T, U, K>(
			this IEnumerable<T> left,
			IEnumerable<U> right,
			Func<T, K> leftKeySelector,
			Func<U, K> rightKeySelector) where K : IComparable<K>
		{
			var sortedLeft = left.OrderBy(leftKeySelector);
			var sortedRight = right.OrderBy(rightKeySelector);

			using (var leftEnumerator = sortedLeft.GetEnumerator())
			using (var rightEnumerator = sortedRight.GetEnumerator())
			{
				for (bool leftHasNext = leftEnumerator.MoveNext(), rightHasNext = rightEnumerator.MoveNext();
					leftHasNext || rightHasNext;
					/* moving will be done in body */)
				{
					if (!rightHasNext)
					{
						do
						{
							yield return (leftEnumerator.Current, default(U));
						} while (leftEnumerator.MoveNext());
						yield break;
					}

					if (!leftHasNext)
					{
						do
						{
							yield return (default(T), rightEnumerator.Current);
						} while (rightEnumerator.MoveNext());
						yield break;
					}

					var leftKey = leftKeySelector(leftEnumerator.Current);
					var rightKey = rightKeySelector(rightEnumerator.Current);
					var comparison = leftKey.CompareTo(rightKey);

					if (comparison == 0)
					{
						yield return (leftEnumerator.Current, rightEnumerator.Current);
						leftHasNext = leftEnumerator.MoveNext();
						rightHasNext = rightEnumerator.MoveNext();
						continue;
					}
					else if (comparison < 0)
					{
						yield return (leftEnumerator.Current, default(U));
						leftHasNext = leftEnumerator.MoveNext();
						continue;
					}
					else /* comparison > 0 */
					{
						yield return (default(T), rightEnumerator.Current);
						rightHasNext = rightEnumerator.MoveNext();
						continue;
					}
				}
			}
		}
	}
}
