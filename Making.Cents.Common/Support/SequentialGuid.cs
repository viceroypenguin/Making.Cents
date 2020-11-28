using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Making.Cents.Common.Support
{
	public static class SequentialGuid
	{
		[DllImport("rpcrt4.dll", SetLastError = true)]
		private static extern int UuidCreateSequential(out Guid guid);

		private static Guid GetNextSystemGuid() =>
			UuidCreateSequential(out var guid) != 0
				? Guid.NewGuid()
				: guid;

		private static Guid Translate(this Guid windowsSequential)
		{
			var s = windowsSequential.ToByteArray();
			var t = new byte[16];

			t[3] = s[0];
			t[2] = s[1];
			t[1] = s[2];
			t[0] = s[3];

			t[5] = s[4];
			t[4] = s[5];

			t[7] = s[6];
			t[6] = s[7];

			t[8] = s[8];
			t[9] = s[9];
			t[10] = s[10];
			t[11] = s[11];
			t[12] = s[12];
			t[13] = s[13];
			t[14] = s[14];
			t[15] = s[15];

			return new Guid(t);
		}

		public static Guid Next() =>
			GetNextSystemGuid().Translate();
	}
}
