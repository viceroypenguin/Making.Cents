using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Making.Cents.AccountsModule.ViewModels;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Ids;
using Making.Cents.Wpf.Common.Contracts;

namespace Making.Cents.AccountsModule.Services
{
	public class AccountRegisterService
	{
		private readonly Func<AccountRegisterViewModel> _newAccountRegisterViewModel;
		private readonly Lazy<IMainWindow> _mainWindow;

		public AccountRegisterService(
			Func<AccountRegisterViewModel> newAccountRegisterViewModel,
			Lazy<IMainWindow> mainWindow)
		{
			_newAccountRegisterViewModel = newAccountRegisterViewModel;
			_mainWindow = mainWindow;
		}

		private readonly Dictionary<AccountId, AccountRegisterViewModel> _accountRegisters = new();
		public void OpenAccountRegister(AccountId accountId)
		{
			var vm = _accountRegisters.GetOrAdd(
				accountId,
				_ => _newAccountRegisterViewModel());

			vm.Initialize(accountId);
			_mainWindow.Value.NavigateTab(vm);
		}
	}
}
