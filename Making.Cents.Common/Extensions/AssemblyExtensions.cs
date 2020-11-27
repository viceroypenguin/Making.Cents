using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Extensions
{
	public static class AssemblyExtensions
	{
		public static string GetEmbeddedResource(this Assembly assembly, string path)
		{
			using (var str = assembly.GetManifestResourceStream(path))
			{
				if (str == null)
					throw new ArgumentOutOfRangeException(nameof(path), "Invalid resource name.");

				using (var sr = new StreamReader(str))
					return sr.ReadToEnd();
			}
		}

		public static async Task<string> GetEmbeddedResourceAsync(this Assembly assembly, string path)
		{
			using (var str = assembly.GetManifestResourceStream(path))
			{
				if (str == null)
					throw new ArgumentOutOfRangeException(nameof(path), "Invalid resource name.");

				using (var sr = new StreamReader(str))
					return await sr.ReadToEndAsync();
			}
		}
	}
}
