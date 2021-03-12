using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using EnumsNET;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Making.Cents.Common.Support;
using Making.Cents.Common.ViewModels;
using Making.Cents.Data.Services;

namespace Making.Cents.Accounts.ViewModels
{
	public class AccountsEditorViewModel : ViewModelBase
	{
		#region Initialization
		private readonly AccountService _accountService;

		public AccountsEditorViewModel(
			AccountService accountService)
		{
			_accountService = accountService;

			AccountTypes = Enums.GetMembers<AccountType>();
			SelectedAccountType = AccountTypes[0];
		}

		public void Initialize()
		{
			var accounts = _accountService.GetAccounts();
			_accountsByType = accounts
				.ToLookup(a => a.AccountType)
				.ToDictionary(
					g => g.Key,
					g => (IReadOnlyList<Account>)g.OrderBy(a => a.Name).ToList());
			RaisePropertyChanged(nameof(Accounts));
		}
		#endregion

		#region Properties
		public LoadingViewModel LoadingViewModel { get; } = new();

		public IReadOnlyList<EnumMember<AccountType>> AccountTypes { get; }
		public EnumMember<AccountType> SelectedAccountType { get; set; }
		private void OnSelectedAccountTypeChanged() =>
			SelectedAccount = default;

		private Dictionary<AccountType, IReadOnlyList<Account>>? _accountsByType;
		public IReadOnlyList<Account> Accounts =>
			_accountsByType?.GetOrDefault(SelectedAccountType.Value)
				?? (IReadOnlyList<Account>)Array.Empty<Account>();
		public Account? SelectedAccount { get; set; }
		private void OnSelectedAccountChanged() =>
			EditAccount = AccountViewModel.Build(SelectedAccount);

		public AccountViewModel? EditAccount { get; set; }
		public bool ShowEditor => EditAccount != null;
		#endregion

		#region Commands
		[Command]
		public void NewAccount()
		{
			SelectedAccount = null;
			EditAccount = new AccountViewModel();
		}

		[MemberNotNullWhen(true, nameof(EditAccount))]
		public bool CanSave() =>
			EditAccount != null
			&& EditAccount.AccountType != null
			&& EditAccount.AccountSubType != null;

		[AsyncCommand]
		public async Task Save()
		{
			if (!CanSave())
				throw new InvalidOperationException("WTF?");

			var account = EditAccount.GetAccount();
			await _accountService.Save(account);

			if (SelectedAccount != null && SelectedAccount.AccountType != account.AccountType)
				_accountsByType![SelectedAccount.AccountType] =
					_accountsByType[SelectedAccount.AccountType]
						.Except(new[] { SelectedAccount })
						.ToArray();

			// maybe unnecessary work here; don't particularly care atm.
			_accountsByType![account.AccountType] =
				_accountsByType.GetOrDefault(account.AccountType, Array.Empty<Account>())
					.Except(new[] { SelectedAccount! })
					.Append(account)
					.OrderBy(x => x.Name)
					.ToList();

			SelectedAccountType = Enums.GetMember(account.AccountType)!;
			RaisePropertyChanged(nameof(Accounts));
			SelectedAccount = account;
		}

		[Command]
		public void Cancel()
		{
			SelectedAccount = null;
			EditAccount = null;
		}
		#endregion

		public class AccountViewModel : ViewModelBase
		{
			#region Properties
			public AccountId AccountId { get; set; } = SequentialGuid.Next();
			public string Name { get; set; } = string.Empty;

			public IReadOnlyList<EnumMember<AccountType>> AccountTypes { get; } =
				Enums.GetMembers<AccountType>();
			public EnumMember<AccountType>? AccountType { get; set; }
			private void OnAccountTypeChanged() => AccountSubType = null;
			public IEnumerable<EnumMember<AccountSubType>>? AccountSubTypes =>
				AccountType == null ? null :
				Enums.GetMembers<AccountSubType>()
					.Where(ast => ast.ToInt32() / 100 == AccountType.ToInt32());
			public EnumMember<AccountSubType>? AccountSubType { get; set; }

			public string? PlaidSource { get; set; }
			public Going.Plaid.Entity.Account? PlaidAccountData { get; set; }

			public bool ShowOnMainScreen { get; set; }
			#endregion

			#region Methods
			public static AccountViewModel? Build(Account? account)
			{
				if (account == null) return null;

				return new AccountViewModel
				{
					AccountId = account.AccountId,
					Name = account.Name,
					AccountType = Enums.GetMember(account.AccountType),
					AccountSubType = Enums.GetMember(account.AccountSubType),
					PlaidSource = account.PlaidSource,
					PlaidAccountData = account.PlaidAccountData,
					ShowOnMainScreen = account.ShowOnMainScreen,
				};
			}

			public Account GetAccount()
			{
				if (AccountType == null || AccountSubType == null)
					throw new InvalidOperationException("Account not ready to be saved.");

				return new Account
				{
					AccountId = AccountId,
					Name = Name,
					AccountType = AccountType.Value,
					AccountSubType = AccountSubType.Value,
					PlaidSource = PlaidSource,
					PlaidAccountData = PlaidAccountData,
					ShowOnMainScreen = ShowOnMainScreen,
				};
			}
			#endregion
		}
	}
}
