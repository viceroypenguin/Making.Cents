using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Making.Cents.Services;
using Making.Cents.Services.Models;

namespace Making.Cents.ViewModels
{
	public class PlaidAccountsViewModel : ViewModelBase
	{
		#region Initialization
		private readonly AccountService _accountService;

		public PlaidAccountsViewModel(
			AccountService accountService)
		{
			_accountService = accountService;
		}

		public async Task InitializeAsync()
		{
			if (!PlaidSources.Any())
				PlaidSources = (await _accountService.GetPlaidSources())
					.ToArray();
			SelectedPlaidSource = PlaidSources.FirstOrDefault();
		}
		#endregion

		#region Properties
		public IReadOnlyList<string> PlaidSources { get; private set; } =
			Array.Empty<string>();
		public string? SelectedPlaidSource { get; set; }

		private readonly Dictionary<string, IReadOnlyList<Account>> _accounts =
			new Dictionary<string, IReadOnlyList<Account>>();
		public IReadOnlyList<Account> Accounts { get; private set; } =
			Array.Empty<Account>();
		#endregion

		#region Commands
		private void OnSelectedPlaidSourceChanged() =>
			_ = LoadAccounts();

		[AsyncCommand]
		public async Task LoadAccounts()
		{
			Accounts = Array.Empty<Account>();
			if (string.IsNullOrWhiteSpace(SelectedPlaidSource))
				return;

			if (!_accounts.ContainsKey(SelectedPlaidSource))
				_accounts[SelectedPlaidSource] = await _accountService.GetPlaidAccounts(SelectedPlaidSource);
			Accounts = _accounts[SelectedPlaidSource];
		}
		#endregion
	}
}
