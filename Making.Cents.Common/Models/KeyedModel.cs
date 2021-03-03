using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Models
{
	public abstract class KeyedModel<TKey> : IEquatable<KeyedModel<TKey>>
		where TKey : IEquatable<TKey>
	{
		protected abstract TKey GetKey();

		public bool Equals(KeyedModel<TKey>? other) =>
			(object?)other != null
			&& this.GetType() == other.GetType()
			&& EqualityComparer<TKey>.Default
				.Equals(this.GetKey(), other.GetKey());

		public override bool Equals(object? obj) =>
			obj is KeyedModel<TKey> other
			&& this.Equals(other);

		public override int GetHashCode() =>
			HashCode.Combine(this.GetType(), this.GetKey());

		public static bool operator ==(KeyedModel<TKey>? a, KeyedModel<TKey>? b) =>
			object.ReferenceEquals(a, b)
			&& (a == null || a.Equals(b));

		public static bool operator !=(KeyedModel<TKey>? a, KeyedModel<TKey>? b) =>
			!(a == b);
	}
}
