using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Making.Cents.AccountsModule.Services;
using Making.Cents.Common.Models;
using Making.Cents.Data.Services;
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.AccountsModule.ViewModels
{
	public class MainWindowAccountsListViewModel : ViewModelBase
	{
		private readonly AccountService _accountService;
		private readonly AccountRegisterService _accountRegisterService;

		public MainWindowAccountsListViewModel(
			AccountService accountService,
			AccountRegisterService accountRegisterService)
		{
			_accountService = accountService;
			_accountRegisterService = accountRegisterService;
		}

		public async Task InitializeAsync()
		{
			using (LoadingViewModel.Wait("Loading accounts..."))
				Accounts = (await _accountService.GetDbAccounts())
					.Where(a => a.ShowOnMainScreen)
					.ToArray();
		}

		public LoadingViewModel LoadingViewModel { get; } = new();
		public IReadOnlyList<Account> Accounts { get; private set; } = null!;
		public Account? SelectedAccount { get; set; }

		private void OnSelectedAccountChanged()
		{
			if (SelectedAccount != null)
				_accountRegisterService.OpenAccountRegister(SelectedAccount.AccountId);
		}
	}
}
