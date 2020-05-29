using System;
using System.Diagnostics.CodeAnalysis;
using Making.Cents.Common.Enums;

namespace Making.Cents.Common.Models
{
	public class Account
	{
		public Guid AccountId { get; set; }
		[DisallowNull] public string? Name { get; set; }
		[DisallowNull] public string? FullName { get; set; }

		public AccountType AccountType { get; set; }
		public AccountSubType AccountSubType { get; set; }

		public Guid? ParentAccountId { get; set; }

		public string? PlaidSource { get; set; }
		public Acklann.Plaid.Entity.Account? PlaidAccountData { get; set; }
	}
}
