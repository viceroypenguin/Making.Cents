using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Making.Cents.Accounts.Services;
using Making.Cents.Common.Models;
using Making.Cents.Common.ViewModels;
using Making.Cents.Data.Services;

namespace Making.Cents.Accounts.ViewModels
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

		public void Initialize()
		{
			using (LoadingViewModel.Wait("Loading accounts..."))
				Accounts = _accountService.GetAccounts()
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
