using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.Accounts.ViewModels;
using Making.Cents.Accounts.Views;
using Making.Cents.Plaid.ViewModels;
using Making.Cents.Plaid.Views;
using Making.Cents.Common.Contracts;
using Making.Cents.Common.ViewModels;

namespace Making.Cents.ViewModels
{
	public class ShellViewModel : ViewModelBase, IMainWindow
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
			MainWindowAccountsListViewModel.Initialize();
			using (LoadingViewModel.Wait("Loading Accounts..."))
				return Task.CompletedTask;
		}
		#endregion

		#region Properties
		public LoadingViewModel LoadingViewModel { get; } = new();
		public MainWindowAccountsListViewModel MainWindowAccountsListViewModel { get; }
		public ObservableCollection<IMainWindowTab> Tabs { get; } = new();
		public IMainWindowTab? CurrentTab { get; set; }
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
			vm.Initialize();

			var aView = new AccountsEditorView
			{
				DataContext = vm,
			};
			aView.ShowDialog();
		}

		public void NavigateTab(IMainWindowTab tab)
		{
			if (!Tabs.Contains(tab))
				Tabs.Add(tab);
			CurrentTab = tab;
		}

		[Command]
		public void CloseTab(IMainWindowTab tab)
		{
			Tabs.Remove(tab);
			tab.Close();
		}
		#endregion
	}
}
