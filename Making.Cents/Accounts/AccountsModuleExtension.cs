using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc;
using Making.Cents.Accounts.Services;
using Making.Cents.Accounts.ViewModels;

namespace Making.Cents
{
	public static class AccountsModuleExtension
	{
		public static Container RegisterAccountModule(this Container container)
		{
			container.Register<AccountRegisterService>(reuse: Reuse.Singleton);
			container.Register<AccountsEditorViewModel>();
			container.Register<MainWindowAccountsListViewModel>();
			return container;
		}
	}
}
