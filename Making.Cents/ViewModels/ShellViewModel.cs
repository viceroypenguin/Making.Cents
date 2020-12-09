using System;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.AccountsModule.ViewModels;
using Making.Cents.AccountsModule.Views;
using Making.Cents.PlaidModule.ViewModels;
using Making.Cents.PlaidModule.Views;

namespace Making.Cents.ViewModels
{
	public class ShellViewModel : ViewModelBase
	{
		private readonly Func<PlaidAccountsViewModel> _newPlaidAccountsViewModel;
		private readonly Func<AccountsEditorViewModel> _newAccountsEditorViewModel;

		public ShellViewModel(
			Func<PlaidAccountsViewModel> newPlaidAccountsViewModel,
			Func<AccountsEditorViewModel> newAccountsEditorViewModel)
		{
			_newPlaidAccountsViewModel = newPlaidAccountsViewModel;
			_newAccountsEditorViewModel = newAccountsEditorViewModel;
		}

		public Task InitializeAsync() => Task.CompletedTask;

		[Command]
		public void ViewPlaidAccounts()
		{
			var vm = _newPlaidAccountsViewModel();
			var aView = new PlaidAccountsView
			{
				DataContext = vm,
			};
			aView.ShowDialog();
		}

		[Command]
		public void EditAccounts()
		{
			var vm = _newAccountsEditorViewModel();
			_ = vm.InitializeAsync();

			var aView = new AccountsEditorView
			{
				DataContext = vm,
			};
			aView.ShowDialog();
		}
	}
}
