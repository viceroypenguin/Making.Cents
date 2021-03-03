using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Making.Cents.Data.Support;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data.Services
{
	public class SecurityService
	{
		#region Initialization
		private readonly Func<DbContext> _context;
		private readonly ILogger _logger;

		public SecurityService(
			Func<DbContext> context,
			ILogger logger)
		{
			_context = context;
			_logger = logger;
		}

		private Dictionary<SecurityId, Security> _securities = null!;
		private MutableLookup<SecurityId, Security> _securityValues = null!;

		public async Task InitializeAsync()
		{
			_logger.LogTrace("Downloading accounts from database.");
			using (var c = _context())
			{
				_securities = await c.Securities
					.Select(s => new Security
					{
						SecurityId = s.SecurityId,
						Name = s.Name,
						Ticker = s.Ticker,
					})
					.ToDictionaryAsync(s => s.SecurityId);

				_securityValues = (await c.SecurityValues
					.Select(sv => new Security
					{
						SecurityId = sv.SecurityId,
						CurrentValueDate = sv.Date,
						CurrentValue = sv.Value,
					})
					.ToListAsync())
					.ToMutableLookup(
						sv => sv.SecurityId,
						sv => sv,
						sv => sv.CurrentValueDate);
			}
		}
		#endregion

		public IEnumerable<Security> GetSecurities() =>
			_securities.Values;

		public Security GetSecurity(SecurityId securityId) =>
			_securities[securityId];
	}
}
