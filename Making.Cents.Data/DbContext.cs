using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data
{
	public partial class DbContext : DataConnection
	{
		private readonly ILogger _logger;

		public DbContext(ILogger logger, IConfigurationRoot configurationRoot)
			: base(
				connectionString: configurationRoot.GetConnectionString("Making.Cents"),
				dataProvider: SqlServerTools.GetDataProvider(
					SqlServerVersion.v2017,
					SqlServerProvider.MicrosoftDataSqlClient))
		{
			_logger = logger;
		}
	}
}
