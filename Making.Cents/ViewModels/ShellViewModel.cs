using System;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.AccountsModule.ViewModels;
using Making.Cents.AccountsModule.Views;
using Making.Cents.PlaidModule.ViewModels;
using Making.Cents.PlaidModule.Views;
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.ViewModels
{
	public class ShellViewModel : ViewModelBase
	{
		#region Initialization
		private readonly Func<PlaidAccountsViewModel> _newPlaidAccountsViewModel;
		private readonly Func<AccountsEditorViewModel> _newAccountsEditorViewModel;

		public ShellViewModel(
			Func<PlaidAccountsViewModel> newPlaidAccountsViewModel,
			Func<AccountsEditorViewModel> newAccountsEditorViewModel,
			MainWindowAccountsListViewModel mainWindowAccountsListViewModel)
		{
			_newPlaidAccountsViewModel = newPlaidAccountsViewModel;
			_newAccountsEditorViewModel = newAccountsEditorViewModel;
			MainWindowAccountsListViewModel = mainWindowAccountsListViewModel;
		}

		public Task InitializeAsync()
		{
			_ = MainWindowAccountsListViewModel.InitializeAsync();
			using (LoadingViewModel.Wait("Loading Accounts..."))
				return Task.CompletedTask;
		}
		#endregion

		#region Properties
		public LoadingViewModel LoadingViewModel { get; } = new();
		public MainWindowAccountsListViewModel MainWindowAccountsListViewModel { get; }
		#endregion

		#region Commands
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
		#endregion
	}
}
