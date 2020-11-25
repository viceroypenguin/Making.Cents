using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc;

namespace Making.Cents
{
	public static class AccountModuleExtension
	{
		public static Container RegisterAccountModule(this Container container)
		{
			return container;
		}
	}
}
