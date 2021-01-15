using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data
{
	public class DbContextOptions
	{
		public string ConnectionString { get; set; } = null!;
	}

	public partial class DbContext : DataConnection
	{
		private readonly ILogger _logger;

		public DbContext(ILogger logger, DbContextOptions options)
			: base(
				connectionString: options.ConnectionString,
				dataProvider: SqlServerTools.GetDataProvider(
					SqlServerVersion.v2017,
					SqlServerProvider.MicrosoftDataSqlClient))
		{
			_logger = logger;
		}
	}
}
