using LinqToDB;
using LinqToDB.Data;
using Making.Cents.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Making.Cents.Data
{
	public partial class DbContext : DataConnection
	{
		private readonly ILogger _logger;

		public DbContext(ILogger logger, IConfigurationRoot configurationRoot)
			: base(
				  connectionString: configurationRoot.GetConnectionString("Making.Cents"),
				  providerName: "SqlServer")
		{
			_logger = logger;
		}

		public ITable<AccountType> AccountTypes => GetTable<AccountType>();
		public ITable<AccountSubType> AccountSubTypes => GetTable<AccountSubType>();
		public ITable<Account> Accounts => GetTable<Account>();
		public ITable<ClearedStatus> ClearedStatuses => GetTable<ClearedStatus>();
		public ITable<Stock> Stocks => GetTable<Stock>();
		public ITable<StockValue> StockValues => GetTable<StockValue>();
		public ITable<Transaction> Transactions => GetTable<Transaction>();
		public ITable<TransactionItem> TransactionItems => GetTable<TransactionItem>();
		public ITable<VersionHistory> VersionHistories => GetTable<VersionHistory>();
	}
}
