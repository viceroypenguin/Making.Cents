using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static Mapper<TSource> Map<TSource>(this TSource source) =>
			new Mapper<TSource>(source);

		private static Dictionary<(Type source, Type target), object> s_initFunc = new();

		public struct Mapper<TSource>
		{
			private readonly TSource _source;

			public Mapper(TSource source) =>
				_source = source;

			public TTarget To<TTarget>()
			{
				var obj = s_initFunc.GetOrDefault((typeof(TSource), typeof(TTarget)));
				if (obj is not Func<TSource, TTarget> func)
				{
					func = BuildInitMap<TTarget>();
					s_initFunc = new Dictionary<(Type source, Type target), object>(s_initFunc)
					{
						[(typeof(TSource), typeof(TTarget))] = func,
					};
				}

				return func(_source);
			}

			private static Func<TSource, TTarget> BuildInitMap<TTarget>()
			{
				throw new NotImplementedException();
			}

			public TTarget To<TTarget>(TTarget destination)
			{
				throw new NotImplementedException();
			}
		}

	}
}
