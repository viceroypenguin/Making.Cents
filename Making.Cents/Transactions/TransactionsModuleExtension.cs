using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc;
using Making.Cents.Transactions.ViewModels;

namespace Making.Cents
{
	public static class TransactionsModuleExtension
	{
		public static Container RegisterTransactionModule(this Container container)
		{
			container.Register<AccountRegisterViewModel>();
			return container;
		}
	}
}
