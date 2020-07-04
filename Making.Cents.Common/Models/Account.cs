using System;
using System.Diagnostics.CodeAnalysis;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Ids;

namespace Making.Cents.Common.Models
{
	public class Account
	{
		public AccountId AccountId { get; set; }
		public string Name { get; set; } = string.Empty;

		public AccountType AccountType { get; set; }
		public AccountSubType AccountSubType { get; set; }

		public string? PlaidSource { get; set; }
		public Acklann.Plaid.Entity.Account? PlaidAccountData { get; set; }

		public decimal Balance { get; set; }
	}
}
