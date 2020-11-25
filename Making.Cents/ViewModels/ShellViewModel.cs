using System;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.PlaidModule.ViewModels;
using Making.Cents.PlaidModule.Views;

namespace Making.Cents.ViewModels
{
	public class ShellViewModel : ViewModelBase
	{
		private readonly Func<PlaidAccountsViewModel> _newPlaidAccountsViewModel;

		public ShellViewModel(Func<PlaidAccountsViewModel> newPlaidAccountsViewModel)
		{
			_newPlaidAccountsViewModel = newPlaidAccountsViewModel;
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
			aView.Show();

			_ = vm.InitializeAsync();
		}
	}
}
