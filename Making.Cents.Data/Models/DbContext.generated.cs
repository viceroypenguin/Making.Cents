//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------

#pragma warning disable 1591

using System;
using System.Collections.Generic;

using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

using Making.Cents.Common.Ids;
using Making.Cents.Data.Converters;

namespace Making.Cents.Data
{
	using Models;
	public partial class DbContext : LinqToDB.Data.DataConnection
	{
		public ITable<Account>                   Accounts            { get { return this.GetTable<Account>(); } }
		public ITable<EnumTable_AccountSubType>  AccountSubTypes     { get { return this.GetTable<EnumTable_AccountSubType>(); } }
		public ITable<EnumTable_AccountType>     AccountTypes        { get { return this.GetTable<EnumTable_AccountType>(); } }
		public ITable<EnumTable_ClearedStatus>   ClearedStatus       { get { return this.GetTable<EnumTable_ClearedStatus>(); } }
		public ITable<EnumTable_TransactionType> TransactionTypes    { get { return this.GetTable<EnumTable_TransactionType>(); } }
		public ITable<Security>                  Securities          { get { return this.GetTable<Security>(); } }
		public ITable<SecurityValue>             SecurityValues      { get { return this.GetTable<SecurityValue>(); } }
		public ITable<Transaction>               Transactions        { get { return this.GetTable<Transaction>(); } }
		public ITable<TransactionBalance>        TransactionBalances { get { return this.GetTable<TransactionBalance>(); } }
		public ITable<TransactionItem>           TransactionItems    { get { return this.GetTable<TransactionItem>(); } }
		public ITable<VersionHistory>            VersionHistories    { get { return this.GetTable<VersionHistory>(); } }
	}
}

namespace Making.Cents.Data.Models
{
	[Table(Schema="dbo", Name="Account")]
	public partial class Account
	{
		[ValueConverter(ConverterType = typeof(AccountIdConverter)), PrimaryKey,                                                        NotNull] public AccountId        AccountId        { get; set; } // uniqueidentifier
		[Column,                                                                                                                        NotNull] public string           Name             { get; set; } // varchar(1000)
		[Column,                                                     ValueConverter(ConverterType = typeof(AccountTypeIdConverter)),    NotNull] public AccountTypeId    AccountTypeId    { get; set; } // int
		[Column,                                                     ValueConverter(ConverterType = typeof(AccountSubTypeIdConverter)), NotNull] public AccountSubTypeId AccountSubTypeId { get; set; } // int
		[Column,                                                        Nullable                                                               ] public string           PlaidSource      { get; set; } // varchar(50)
		[Column,                                                        Nullable                                                               ] public string           PlaidAccountData { get; set; } // varchar(max)
		[Column,                                                                                                                        NotNull] public bool             ShowOnMainScreen { get; set; } // bit

		#region Associations

		/// <summary>
		/// FK_Account_AccountSubType
		/// </summary>
		[Association(ThisKey="AccountSubTypeId", OtherKey="AccountSubTypeId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_Account_AccountSubType", BackReferenceName="Accounts")]
		public EnumTable_AccountSubType AccountSubType { get; set; }

		/// <summary>
		/// FK_Account_AccountType
		/// </summary>
		[Association(ThisKey="AccountTypeId", OtherKey="AccountTypeId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_Account_AccountType", BackReferenceName="Accounts")]
		public EnumTable_AccountType AccountType { get; set; }

		/// <summary>
		/// FK_TransactionItem_Account_BackReference
		/// </summary>
		[Association(ThisKey="AccountId", OtherKey="AccountId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<TransactionItem> TransactionItems { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="AccountSubType")]
	public partial class EnumTable_AccountSubType
	{
		[ValueConverter(ConverterType = typeof(AccountSubTypeIdConverter)), PrimaryKey,  NotNull] public AccountSubTypeId AccountSubTypeId { get; set; } // int
		[Column,                                                               Nullable         ] public string           Name             { get; set; } // varchar(50)

		#region Associations

		/// <summary>
		/// FK_Account_AccountSubType_BackReference
		/// </summary>
		[Association(ThisKey="AccountSubTypeId", OtherKey="AccountSubTypeId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Account> Accounts { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="AccountType")]
	public partial class EnumTable_AccountType
	{
		[ValueConverter(ConverterType = typeof(AccountTypeIdConverter)), PrimaryKey,  NotNull] public AccountTypeId AccountTypeId { get; set; } // int
		[Column,                                                            Nullable         ] public string        Name          { get; set; } // varchar(50)

		#region Associations

		/// <summary>
		/// FK_Account_AccountType_BackReference
		/// </summary>
		[Association(ThisKey="AccountTypeId", OtherKey="AccountTypeId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Account> Accounts { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="ClearedStatus")]
	public partial class EnumTable_ClearedStatus
	{
		[ValueConverter(ConverterType = typeof(ClearedStatusIdConverter)), PrimaryKey,  NotNull] public ClearedStatusId ClearedStatusId { get; set; } // int
		[Column,                                                              Nullable         ] public string          Name            { get; set; } // varchar(50)

		#region Associations

		/// <summary>
		/// FK_TransactionItem_ClearedStatus_BackReference
		/// </summary>
		[Association(ThisKey="ClearedStatusId", OtherKey="ClearedStatusId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<TransactionItem> TransactionItems { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="TransactionType")]
	public partial class EnumTable_TransactionType
	{
		[ValueConverter(ConverterType = typeof(TransactionTypeIdConverter)), PrimaryKey,  NotNull] public TransactionTypeId TransactionTypeId { get; set; } // int
		[Column,                                                                Nullable         ] public string            Name              { get; set; } // varchar(50)

		#region Associations

		/// <summary>
		/// FK_Transaction_TransactionType_BackReference
		/// </summary>
		[Association(ThisKey="TransactionTypeId", OtherKey="TransactionTypeId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Transaction> Transactions { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="Security")]
	public partial class Security
	{
		[ValueConverter(ConverterType = typeof(SecurityIdConverter)), PrimaryKey, NotNull] public SecurityId SecurityId { get; set; } // uniqueidentifier
		[Column,                                                                  NotNull] public string     Ticker     { get; set; } // varchar(50)
		[Column,                                                                  NotNull] public string     Name       { get; set; } // varchar(200)

		#region Associations

		/// <summary>
		/// FK_SecurityValue_Security_BackReference
		/// </summary>
		[Association(ThisKey="SecurityId", OtherKey="SecurityId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<SecurityValue> SecurityValues { get; set; }

		/// <summary>
		/// FK_TransactionItem_Security_BackReference
		/// </summary>
		[Association(ThisKey="SecurityId", OtherKey="SecurityId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<TransactionItem> TransactionItems { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="SecurityValue")]
	public partial class SecurityValue
	{
		[ValueConverter(ConverterType = typeof(SecurityIdConverter)), PrimaryKey(1), NotNull] public SecurityId SecurityId { get; set; } // uniqueidentifier
		[                                                             PrimaryKey(2), NotNull] public DateTime   Date       { get; set; } // date
		[Column,                                                                     NotNull] public decimal    Value      { get; set; } // money

		#region Associations

		/// <summary>
		/// FK_SecurityValue_Security
		/// </summary>
		[Association(ThisKey="SecurityId", OtherKey="SecurityId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_SecurityValue_Security", BackReferenceName="SecurityValues")]
		public Security Security { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="Transaction")]
	public partial class Transaction
	{
		[ValueConverter(ConverterType = typeof(TransactionIdConverter)), PrimaryKey,                                                         NotNull] public TransactionId     TransactionId     { get; set; } // uniqueidentifier
		[Column,                                                                                                                             NotNull] public DateTime          Date              { get; set; } // date
		[Column,                                                         ValueConverter(ConverterType = typeof(TransactionTypeIdConverter)), NotNull] public TransactionTypeId TransactionTypeId { get; set; } // int
		[Column,                                                                                                                             NotNull] public string            Description       { get; set; } // varchar(255)
		[Column,                                                            Nullable                                                                ] public string            Memo              { get; set; } // varchar(255)

		#region Associations

		/// <summary>
		/// FK_TransactionItem_Transaction_BackReference
		/// </summary>
		[Association(ThisKey="TransactionId", OtherKey="TransactionId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<TransactionItem> TransactionItems { get; set; }

		/// <summary>
		/// FK_Transaction_TransactionType
		/// </summary>
		[Association(ThisKey="TransactionTypeId", OtherKey="TransactionTypeId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_Transaction_TransactionType", BackReferenceName="Transactions")]
		public EnumTable_TransactionType TransactionType { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="TransactionBalance", IsView=true)]
	public partial class TransactionBalance
	{
		[Column, ValueConverter(ConverterType = typeof(TransactionIdConverter)), NotNull] public TransactionId TransactionId { get; set; } // uniqueidentifier
		[Column,    Nullable                                                            ] public decimal?      Balance       { get; set; } // money
		[Column,    Nullable                                                            ] public long?         ItemCount     { get; set; } // bigint
	}

	[Table(Schema="dbo", Name="TransactionItem")]
	public partial class TransactionItem
	{
		[ValueConverter(ConverterType = typeof(TransactionIdConverter)),     PrimaryKey(1),                                                    NotNull] public TransactionId     TransactionId        { get; set; } // uniqueidentifier
		[ValueConverter(ConverterType = typeof(TransactionItemIdConverter)), PrimaryKey(2),                                                    NotNull] public TransactionItemId TransactionItemId    { get; set; } // uniqueidentifier
		[Column,                                                             ValueConverter(ConverterType = typeof(AccountIdConverter)),       NotNull] public AccountId         AccountId            { get; set; } // uniqueidentifier
		[Column,                                                             ValueConverter(ConverterType = typeof(SecurityIdConverter)),      NotNull] public SecurityId        SecurityId           { get; set; } // uniqueidentifier
		[Column,                                                                                                                               NotNull] public decimal           Shares               { get; set; } // money
		[Column,                                                                                                                               NotNull] public decimal           Amount               { get; set; } // money
		[Column(SkipOnInsert=true, SkipOnUpdate=true),                                                                                         NotNull] public decimal           PerShare             { get; set; } // money
		[Column,                                                             ValueConverter(ConverterType = typeof(ClearedStatusIdConverter)), NotNull] public ClearedStatusId   ClearedStatusId      { get; set; } // int
		[Column,                                                                Nullable                                                              ] public string            Memo                 { get; set; } // varchar(250)
		[Column,                                                                Nullable                                                              ] public string            PlaidTransactionData { get; set; } // varchar(50)

		#region Associations

		/// <summary>
		/// FK_TransactionItem_Account
		/// </summary>
		[Association(ThisKey="AccountId", OtherKey="AccountId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_TransactionItem_Account", BackReferenceName="TransactionItems")]
		public Account Account { get; set; }

		/// <summary>
		/// FK_TransactionItem_ClearedStatus
		/// </summary>
		[Association(ThisKey="ClearedStatusId", OtherKey="ClearedStatusId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_TransactionItem_ClearedStatus", BackReferenceName="TransactionItems")]
		public EnumTable_ClearedStatus ClearedStatus { get; set; }

		/// <summary>
		/// FK_TransactionItem_Security
		/// </summary>
		[Association(ThisKey="SecurityId", OtherKey="SecurityId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_TransactionItem_Security", BackReferenceName="TransactionItems")]
		public Security Security { get; set; }

		/// <summary>
		/// FK_TransactionItem_Transaction
		/// </summary>
		[Association(ThisKey="TransactionId", OtherKey="TransactionId", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="FK_TransactionItem_Transaction", BackReferenceName="TransactionItems")]
		public Transaction Transaction { get; set; }

		#endregion
	}

	[Table(Schema="dbo", Name="VersionHistory")]
	public partial class VersionHistory
	{
		[ValueConverter(ConverterType = typeof(VersionHistoryIdConverter)), PrimaryKey, Identity] public VersionHistoryId VersionHistoryId { get; set; } // int
		[Column,                                                            NotNull             ] public string           SqlFile          { get; set; } // varchar(50)
		[Column,                                                            NotNull             ] public DateTimeOffset   Timestamp        { get; set; } // datetimeoffset(7)
	}
}

#pragma warning restore 1591

