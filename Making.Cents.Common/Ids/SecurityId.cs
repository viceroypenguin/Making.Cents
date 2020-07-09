using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
	public partial struct SecurityId
	{
		public static implicit operator int(SecurityId securityId) =>
			securityId.Value;
		public static explicit operator SecurityId(int securityId) =>
			new SecurityId(securityId);
	}
}
