using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Models;
using Microsoft.Extensions.Logging;

namespace Making.Cents.Qif
{
	public class Qif
	{
		#region Initialization
		private Qif() { }

		public static async Task<Qif> ReadFile(string fileName, ILogger<Qif>? logger = null)
		{
			using (var stream = new StreamReader(fileName))
				return await ReadStream(stream, logger);
		}

		public static async Task<Qif> ReadStream(StreamReader stream, ILogger<Qif>? logger = null) =>
			await new QifParser(stream, logger).Parse();
		#endregion

		#region Properties
		public Dictionary<string, Account> Accounts { get; } = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, Stock> Stocks { get; } = new Dictionary<string, Stock>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Parser
		private class QifParser
		{
			private readonly Qif _qif = new Qif();
			private readonly StreamReader _stream;
			private readonly ILogger<Qif>? _logger;

			public QifParser(StreamReader stream, ILogger<Qif>? logger)
			{
				_stream = stream;
				_logger = logger;
			}

			private List<string> _record = null!;
			private int _lineNumber = 0;

			public async Task<Qif> Parse()
			{
				await GetNextRecord();

				while (true)
				{
					if (_record == null)
						return _qif;

					switch (_record[0])
					{
						case "!Account": await ParseAccounts(); break;
						case "!Type:Bank": await ParseNonInvestmentTransactions(); break;
						case "!Type:Cash": await ParseNonInvestmentTransactions(); break;
						case "!Type:CCard": await ParseNonInvestmentTransactions(); break;
						case "!Type:Invst": await ParseInvestmentTransactions(); break;
						case "!Type:Oth A": await ParseNonInvestmentTransactions(); break;
						case "!Type:Oth L": await ParseNonInvestmentTransactions(); break;
						case "!Type:Cat": await ParseAccounts(); break;
						case "!Type:Class": await ParseClassifications(); break;
						case "!Type:Memorized": await ParseMemorizedTransactions(); break;
						case "!Type:Prices": await ParseSecurityPrice(); break;
						case "!Type:Security": await ParseSecurity(); break;

						default:
							throw new InvalidOperationException($"Invalid .qif file. Unable to parse section type '{_record[0]}'");
					}
				}
			}

			private async Task GetNextRecord()
			{
				_record = new List<string>();
				while (true)
				{
					var line = (await _stream.ReadLineAsync())?.Trim();
					_logger?.LogTrace("{lineNumber}: {line}", ++_lineNumber, line);

					if (line == null)
						return;

					if (line == "^")
						return;

					if (line == "!Option:AutoSwitch")
						continue;
					if (line == "!Clear:AutoSwitch")
						continue;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					_record.Add(line);
				}
			}

			private Account? _currentAccount;
			private Account GetCurrentAccount() =>
				_currentAccount ?? throw new InvalidOperationException("Unable to parse account. Expected current account here, none found.");

			private async Task ParseAccounts()
			{
				_record.RemoveAt(0);
				while (true)
				{
					if (_record[0][0] == '!')
						return;

					var dict = _record.Where(x => x[0] != 'B').ToDictionary(x => x[0], x => x[1..]);
					_currentAccount = _qif.Accounts.GetOrAdd(
						dict['N'],
						name =>
						{
							var account = new Account
							{
								Name = name,
							};

							if (dict.ContainsKey('I'))
							{
								(account.AccountType, account.AccountSubType) =
									(AccountType.Income, AccountSubType.Income);
							}
							else if (dict.ContainsKey('E'))
							{
								(account.AccountType, account.AccountSubType) =
									(AccountType.Expense, AccountSubType.Expense);
							}
							else if (dict.TryGetValue('T', out var type))
							{
								if (!string.IsNullOrWhiteSpace(type))
									(account.AccountType, account.AccountSubType) =
										ParseAccountType(type);
							}

							return account;
						});

					await GetNextRecord();
				}
			}

			private (AccountType, AccountSubType) ParseAccountType(string detail) =>
				detail switch
				{
					"Cash" => (AccountType.Asset, AccountSubType.Cash),
					"Bank" => (AccountType.Asset, AccountSubType.Checking),
					"Investment" => (AccountType.Asset, AccountSubType.Brokerage),
					"Oth A" => (AccountType.Asset, AccountSubType.OtherAsset),
					"401(k)/403(b)" => (AccountType.Asset, AccountSubType.Retirement),
					"IRA" => (AccountType.Asset, AccountSubType.Retirement),
					"Hsa" => (AccountType.Asset, AccountSubType.Hsa),
					"CCard" => (AccountType.Liability, AccountSubType.CreditCard),
					"Oth L" => (AccountType.Liability, AccountSubType.OtherLiability),
					_ => throw new InvalidOperationException($"Unknown account type: '{detail}'."),
				};

			private Task ParseInvestmentTransactions()
			{
				throw new NotImplementedException();
			}

			private Task ParseNonInvestmentTransactions()
			{
				throw new NotImplementedException();
			}

			private Task ParseClassifications()
			{
				throw new NotImplementedException();
			}

			private Task ParseMemorizedTransactions()
			{
				throw new NotImplementedException();
			}

			private Task ParseSecurityPrice()
			{
				throw new NotImplementedException();
			}

			private async Task ParseSecurity()
			{
				_record.RemoveAt(0);
				while (true)
				{
					if (_record[0][0] == '!')
						return;

					var dict = _record.ToDictionary(x => x[0], x => x[1..]);
					_qif.Stocks.GetOrAdd(
						dict['N'],
						_ => new Stock
						{
							Name = dict['N'],
							Ticker = dict.GetOrDefault('S', "____"),
						});

					await GetNextRecord();
				}
			}
		}
		#endregion
	}
}
