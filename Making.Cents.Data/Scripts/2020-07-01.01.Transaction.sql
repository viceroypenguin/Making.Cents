create table Security
(
	SecurityId uniqueidentifier not null
		constraint [PK_Security] primary key,
	Ticker varchar(50) not null,
	Name varchar(200) not null,
	PlaidSource varchar(50) null,
	IsCashEquivalent bit not null,
);

insert Security values('ca000000-0000-0000-0000-000000000000', 'CASH', 'CASH', null, 1);

create table SecurityValue
(
	SecurityId uniqueidentifier not null
		constraint [FK_SecurityValue_Security]
		foreign key references Security,
	[Date] date not null,
	[Value] money not null,

	constraint [PK_SecurityValue]
		primary key (SecurityId, [Date]),
);

insert SecurityValue values ('ca000000-0000-0000-0000-000000000000', '2000-01-01', 1.00);

create table TransactionType
(
	TransactionTypeId int not null
		constraint [PK_TransactionType]
		primary key,
	Name varchar(50),
);

create table [Transaction]
(
	TransactionId uniqueidentifier not null
		constraint [PK_Transaction] primary key,
	[Date] Date not null,
	TransactionTypeId int not null
		constraint [FK_Transaction_TransactionType]
		foreign key references TransactionType,
	Description varchar(255) not null,
	Memo varchar(255) null,
);

create table ClearedStatus
(
	ClearedStatusId int not null
		constraint [PK_ClearedStatus]
		primary key,
	Name varchar(50),
);

create table TransactionItem
(
	TransactionId uniqueidentifier not null
		constraint [FK_TransactionItem_Transaction]
		foreign key references [Transaction],
	TransactionItemId uniqueidentifier not null,
	AccountId uniqueidentifier not null
		constraint [FK_TransactionItem_Account]
		foreign key references Account,
	SecurityId uniqueidentifier not null
		constraint [FK_TransactionItem_Security]
		foreign key references Security,
	Shares money not null,
	Amount money not null,
	PerShare as isnull(Amount / nullif(Shares, 0), 0) persisted not null,

	ClearedStatusId int not null
		constraint [FK_TransactionItem_ClearedStatus]
		foreign key references ClearedStatus,
	Memo varchar(250) null,
	PlaidTransactionData varchar(50) null,

	constraint [PK_TransactionItem] 
		primary key (TransactionId, TransactionItemId),

	constraint [CK_TransactionItem_Cash_PerShare]
		check (SecurityId != 'ca000000-0000-0000-0000-000000000000' or PerShare = 1),
);

create index [IX_TransactionItem_AccountId]
on TransactionItem(AccountId)
include (TransactionId, TransactionItemId, SecurityId, Shares, Amount, PerShare, ClearedStatusId, Memo);
go

create or alter view TransactionBalance
with schemabinding
as
select t.TransactionId, sum(ti.Amount) Balance, count_big(*) ItemCount
from dbo.[Transaction] t
	inner join dbo.[TransactionItem] ti
		on t.TransactionId = ti.TransactionId
group by t.TransactionId;
go

create unique clustered index [IX_C_TransactionBalance]
on TransactionBalance(TransactionId);
go

create index [IX_TransactionBalance_OutOfBalanceTransactions]
on TransactionBalance(Balance, TransactionId);
go
