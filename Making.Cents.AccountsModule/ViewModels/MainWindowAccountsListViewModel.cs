using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Making.Cents.Common.Models;
using Making.Cents.Data.Services;
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.AccountsModule.ViewModels
{
	public class MainWindowAccountsListViewModel : ViewModelBase
	{
		private readonly AccountService _accountService;

		public MainWindowAccountsListViewModel(AccountService accountService)
		{
			_accountService = accountService;
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
	}
}
