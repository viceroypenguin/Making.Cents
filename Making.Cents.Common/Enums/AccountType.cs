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
		Equity = 5,
	}

	public enum AccountSubType
	{
		Cash = 100,
		Checking = 101,
		Savings = 102,

		Brokerage = 103,
		Retirement = 104,
		Hsa = 105,

		House = 106,
		Vehicle = 107,
		OtherAsset = 108,

		CreditCard = 200,
		Loan = 201,
		Mortgage = 202,
		Heloc = 203,
		OtherLiability = 204,

		Income = 300,

		Expense = 400,

		Capital = 500,
	}
}
