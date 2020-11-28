using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
	public partial struct SecurityId
	{
		public static explicit operator Guid(SecurityId securityId) =>
			securityId.Value;
		public static implicit operator SecurityId(Guid securityId) =>
			new SecurityId(securityId);
	}
}
