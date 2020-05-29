using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Common.Enums
{
	public enum AccountType
	{
		Asset = 1,
		Liability = 2,
		Income = 3,
		Expense = 4,
	}

	public enum AccountSubType
	{
		Cash = 1,
		Checking = 2,
		Savings = 3,

		Brokerage = 4,
		Retirement = 5,
		Hsa = 6,

		House = 7,
		Vehicle = 8,
		OtherAsset = 9,

		CreditCard = 10,
		Loan = 11,
		Mortgage = 12,
		Heloc = 13,
		OtherLiability = 14,

		Income = 15,

		Expense = 16,
	}
}
