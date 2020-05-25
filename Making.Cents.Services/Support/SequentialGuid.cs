using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Making.Cents.Services.Support
{
	public static class SequentialGuid
	{
		[DllImport("rpcrt4.dll", SetLastError = true)]
		public static extern int UuidCreateSequential(out Guid guid);

		private static Guid GetNextSystemGuid() =>
			UuidCreateSequential(out var guid) != 0
				? Guid.NewGuid()
				: guid;

		private static Sander.SequentialGuid.SequentialGuid? s_sequentialGuid;

		public static Guid Next() =>
			(s_sequentialGuid ??=
				new Sander.SequentialGuid.SequentialGuid(
					GetNextSystemGuid()))
				.Next();
	}
}
