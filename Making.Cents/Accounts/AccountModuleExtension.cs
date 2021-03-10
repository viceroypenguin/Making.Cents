using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc;
using Making.Cents.AccountsModule.Services;
using Making.Cents.AccountsModule.ViewModels;

namespace Making.Cents
{
	public static class AccountModuleExtension
	{
		public static Container RegisterAccountModule(this Container container)
		{
			container.Register<AccountRegisterService>(reuse: Reuse.Singleton);
			container.Register<AccountRegisterViewModel>();
			container.Register<AccountsEditorViewModel>();
			container.Register<MainWindowAccountsListViewModel>();
			return container;
		}
	}
}
