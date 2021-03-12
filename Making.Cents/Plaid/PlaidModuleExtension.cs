using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc;
using Making.Cents.Plaid.ViewModels;
using Making.Cents.Plaid.Views;

namespace Making.Cents
{
	public static class PlaidModuleExtension
	{
		public static Container RegisterPlaidModule(this Container container)
		{
			container.Register<PlaidAccountsViewModel>(setup: Setup.With(openResolutionScope: true));
			container.Register<PlaidLinkWindow>();
			return container;
		}
	}
}
