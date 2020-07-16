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
	}
}
