using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Making.Cents.Common.Ids;
using Making.Cents.Data.Services;
using Making.Cents.Wpf.Common.Contracts;
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.AccountsModule.ViewModels
{
	public class AccountRegisterViewModel : ViewModelBase, IMainWindowTab
	{
		private readonly AccountService _accountService;
		private readonly TransactionService _transactionService;

		public AccountRegisterViewModel(
			AccountService accountService,
			TransactionService transactionService)
		{
			_accountService = accountService;
			_transactionService = transactionService;
		}

		private bool _isInitialized;


		public async Task InitializeAsync(AccountId accountId)
		{
			if (_isInitialized)
				return;

			Title = accountId.ToString();
			await Task.Yield();
			_isInitialized = true;
		}

		public string Title { get; private set; } = null!;
		public LoadingViewModel LoadingViewModel { get; } = new();

		public bool CanClose() => false;
		public void Close() => _isInitialized = false;
	}
}
