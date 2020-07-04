create table Stock
(
	StockId int identity(0,1) not null
		constraint [PK_Stock] primary key,
	Ticker varchar(10) not null,
	Name varchar(200) not null,
);

insert Stock values('CASH', 'CASH');

create table StockValue
(
	StockId int not null
		constraint [FK_StockValue_Stock]
		foreign key references Stock,
	[Date] date not null,
	[Value] money not null,

	constraint [PK_StockValue]
		primary key (StockId, [Date]),
);

insert StockValue values (0, '2020-01-01', 1.00);

create table [Transaction]
(
	TransactionId int identity(1,1) not null
		constraint [PK_Transaction] primary key,
	[Date] Date not null,
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
	TransactionId int not null
		constraint [FK_TransactionItem_Transaction]
		foreign key references [Transaction],
	TransactionItemId int identity(1,1) not null,
	AccountId int not null
		constraint [FK_TransactionItem_Account]
		foreign key references Account,
	StockId int not null
		constraint [FK_TransactionItem_Stock]
		foreign key references Stock,
	Shares money not null,
	Amount money not null,
	PerShare as Amount / Shares persisted not null,

	ClearedStatusId int not null
		constraint [FK_TransactionItem_ClearedStatus]
		foreign key references ClearedStatus,
	Memo varchar(250) null,

	constraint [PK_TransactionItem] 
		primary key (TransactionId, TransactionItemId),

	constraint [CK_TransactionItem_Cash_PerShare]
		check (StockId != 0 or PerShare = 1),
);

create index [IX_TransactionItem_AccountId]
on TransactionItem(AccountId)
include (TransactionId, TransactionItemId, StockId, Shares, Amount, PerShare, ClearedStatus, Memo);
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
