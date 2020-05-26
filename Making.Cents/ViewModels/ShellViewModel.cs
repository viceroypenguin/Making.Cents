using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.Views;

namespace Making.Cents.ViewModels
{
	public class ShellViewModel : ViewModelBase
	{
		private readonly PlaidAccountsViewModel _plaidAccountsViewModel;

		public ShellViewModel(PlaidAccountsViewModel plaidAccountsViewModel)
		{
			_plaidAccountsViewModel = plaidAccountsViewModel;
		}

		public Task InitializeAsync() => Task.CompletedTask;

		[Command]
		public void ViewPlaidAccounts()
		{
			var aView = new PlaidAccountsView
			{
				DataContext = _plaidAccountsViewModel,
			};
			aView.Show();

			_ = _plaidAccountsViewModel.InitializeAsync();
		}
	}
}
