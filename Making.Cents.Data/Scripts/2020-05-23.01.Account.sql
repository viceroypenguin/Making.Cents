create table AccountType
(
	AccountTypeId int not null
		constraint [PK_AccountType]
		primary key,
	Name varchar(50),
);

create table AccountSubType
(
	AccountSubTypeId int not null
		constraint [PK_AccountSubType]
		primary key,
	Name varchar(50),
);

create table Account
(
	AccountId int identity(1,1) not null
		constraint [PK_Account]
		primary key,
	Name varchar(1000) not null,
	FullName varchar(1000) not null,
	
	AccountTypeId int not null
		constraint [FK_Account_AccountType]
		foreign key references AccountType,
	AccountSubTypeId int not null
		constraint [FK_Account_AccountSubType]
		foreign key references AccountSubType,

	ParentAccountId int null
		constraint [FK_Account_Account]
		foreign key references Account,

	PlaidSource varchar(50) null,
	PlaidAccountData varchar(max) null
		constraint [CK_Account_PlaidAccountData_IsJson]
		check (PlaidAccountData is null or isjson(PlaidAccountData) = 1),
);

create unique index [UK_Account_FullName]
on Account(FullName);
