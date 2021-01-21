using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data.Services
{
	public class TransactionService
	{
		private readonly Func<DbContext> _context;
		private readonly ILogger _logger;

		public TransactionService(
			Func<DbContext> context,
			ILogger logger)
		{
			_context = context;
			_logger = logger;
		}
	}
}
