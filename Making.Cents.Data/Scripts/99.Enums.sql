merge into AccountType
using (values
	(1, 'Asset'),
	(2, 'Liability'),
	(3, 'Income'),
	(4, 'Expense')
	) x(id, name)
on AccountType.AccountTypeId = x.id
when not matched by target
	then insert(AccountTypeId, Name)
	values (x.id, x.name)
when matched
	then update 
	set Name = x.name
when not matched by source
	then delete;

merge into AccountSubType
using (values
	(1, 'Cash'),
	(2, 'Checking'),
	(3, 'Savings'),

	(4, 'Brokerage'),
	(5, 'Retirement'),
	(6, 'Hsa'),

	(7, 'House'),
	(8, 'Vehicle'),
	(9, 'OtherAsset'),

	(10, 'CreditCard'),
	(11, 'Loan'),
	(12, 'Mortgage'),
	(13, 'Heloc'),
	(14, 'OtherLiability'),

	(15, 'Income'),

	(16, 'Expense')
	) x(id, name)
on AccountSubType.AccountSubTypeId = x.id
when not matched by target
	then insert(AccountSubTypeId, Name)
	values (x.id, x.name)
when matched
	then update 
	set Name = x.name
when not matched by source
	then delete;
