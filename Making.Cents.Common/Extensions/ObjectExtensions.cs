using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static Mapper<TSource> Map<TSource>(this TSource source) =>
			new Mapper<TSource>(source);

		private static ConcurrentDictionary<(Type source, Type target), object> s_initFunc = new();
		private static ConcurrentDictionary<(Type source, Type target), object> s_copyFunc = new();

		public struct Mapper<TSource>
		{
			private readonly TSource _source;

			public Mapper(TSource source) =>
				_source = source;

			public TTarget To<TTarget>()
				where TTarget : new()
			{
				var func = (Func<TSource, TTarget>)s_initFunc.GetOrAdd(
					(typeof(TSource), typeof(TTarget)),
					_ => BuildInitMap<TTarget>());

				return func(_source);
			}

			private static Func<TSource, TTarget> BuildInitMap<TTarget>()
				where TTarget : new()
			{
				var p = Expression.Parameter(typeof(TSource), "source");

				var mappings = typeof(TSource).GetProperties()
					.FullOuterJoin(
						typeof(TTarget).GetProperties(),
						l => l.Name,
						r => r.Name)
					.Where(x => x.left != default && x.right != default)
					.Select(x =>
						Expression.Bind(
							x.right!,
							Expression.MakeMemberAccess(
								p, x.left!)));

				return Expression
					.Lambda<Func<TSource, TTarget>>(
						Expression.MemberInit(
							Expression.New(typeof(TTarget)),
							mappings),
						p)
					.Compile();
			}

			public TTarget To<TTarget>(TTarget destination)
			{
				var func = (Action<TSource, TTarget>)s_copyFunc.GetOrAdd(
					(typeof(TSource), typeof(TTarget)),
					_ => BuildCopyMap<TTarget>());

				func(_source, destination);
				return destination;
			}

			private static Action<TSource, TTarget> BuildCopyMap<TTarget>()
			{
				var src = Expression.Parameter(typeof(TSource), "source");
				var tgt = Expression.Parameter(typeof(TTarget), "target");

				var mappings = typeof(TSource).GetProperties()
					.FullOuterJoin(
						typeof(TTarget).GetProperties(),
						l => l.Name,
						r => r.Name)
					.Where(x => x.left != default && x.right != default)
					.Select(x =>
						Expression.Assign(
							Expression.MakeMemberAccess(
								tgt, x.right!),
							Expression.MakeMemberAccess(
								src, x.left!)));

				return Expression
					.Lambda<Action<TSource, TTarget>>(
						Expression.Block(mappings),
						src,
						tgt)
					.Compile();
			}
		}
	}
}
