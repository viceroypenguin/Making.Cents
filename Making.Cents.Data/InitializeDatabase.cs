using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;
using LinqToDB;
using LinqToDB.Data;
using Making.Cents.Common.Extensions;
using Making.Cents.Data.Models;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Data
{
	public partial class DbContext : DataConnection
	{
		public void InitializeDatabase()
		{
			CommandTimeout = 600;

			EnsureVersionHistoryExists();

			RunChangeScripts();
			RunEnumScript();
		}

		#region Main Functions
		private void EnsureVersionHistoryExists() =>
			// script does validation; always run script
			ExecuteScript("00.VersionHistory.sql");

		private void RunChangeScripts()
		{
			var scripts = GetEmbeddedScripts();
			var executedScripts = VersionHistories
				.Select(s => s.SqlFile)
				.ToList();

			var scriptsToRun = scripts
				.Except(executedScripts)
				.Except(new[] { "00.VersionHistory.sql", "99.Enums.sql", })
				.OrderBy(s => s)
				.ToList();

			foreach (var s in scriptsToRun)
			{
				using (var ts = BeginTransaction())
				{
					ExecuteScript(s);
					ts.Commit();
				}

				this.Insert(
					new VersionHistory()
					{
						SqlFile = s,
						Timestamp = DateTime.Now,
					});
			}
		}

		private void RunEnumScript()
		{
			AccountTypes
				.Merge().Using(Enums.GetMembers<Common.Enums.AccountType>())
				.On((dst, src) => dst.AccountTypeId == src.Value)
				.InsertWhenNotMatched(src => new EnumTable_AccountType { AccountTypeId = src.Value, Name = src.Name, })
				.DeleteWhenNotMatchedBySource()
				.Merge();

			AccountSubTypes
				.Merge().Using(Enums.GetMembers<Common.Enums.AccountSubType>())
				.On((dst, src) => dst.AccountSubTypeId == src.Value)
				.InsertWhenNotMatched(src => new EnumTable_AccountSubType { AccountSubTypeId = src.Value, Name = src.Name, })
				.DeleteWhenNotMatchedBySource()
				.Merge();

			ClearedStatus
				.Merge().Using(Enums.GetMembers<Common.Enums.ClearedStatus>())
				.On((dst, src) => dst.ClearedStatusId == src.Value)
				.InsertWhenNotMatched(src => new EnumTable_ClearedStatus { ClearedStatusId = src.Value, Name = src.Name, })
				.DeleteWhenNotMatchedBySource()
				.Merge();
		}
		#endregion

		#region Execute Script
		private static readonly Regex s_sqlBlocks = new Regex(@"^[gG][oO]\r?$", RegexOptions.Compiled | RegexOptions.Multiline);
		private void ExecuteScript(string scriptName)
		{
			try
			{
				var script = GetSqlScript(scriptName);
				foreach (var b in s_sqlBlocks.Split(script).Where(s => !string.IsNullOrWhiteSpace(s)))
					this.Execute(b);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to run script '{script}'.", scriptName);
				throw;
			}
		}
		#endregion

		#region Embedded Scripts
		private const string ResourcePrefix = "Making.Cents.Data.Scripts.";
		private static IList<string> GetEmbeddedScripts() =>
			Assembly.GetExecutingAssembly()
				.GetManifestResourceNames()
				.Where(s => Path.GetExtension(s).Equals(".sql", StringComparison.OrdinalIgnoreCase))
				.Select(s => s.Replace(ResourcePrefix, ""))
				.ToList();

		private static string GetSqlScript(string scriptName)
		{
			if (!scriptName.StartsWith(ResourcePrefix))
				scriptName = ResourcePrefix + scriptName;

			return Assembly.GetExecutingAssembly().GetEmbeddedResource(scriptName);
		}
		#endregion
	}
}
