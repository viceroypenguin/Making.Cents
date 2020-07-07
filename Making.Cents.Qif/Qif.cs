using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Making.Cents.Common.Enums;
using Making.Cents.Common.Extensions;
using Making.Cents.Common.Ids;
using Making.Cents.Common.Models;
using Microsoft.Extensions.Logging;

using static MoreLinq.Extensions.SegmentExtension;

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

			private readonly Dictionary<string, Account> _accounts = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);
			private readonly Dictionary<string, Stock> _stocks = new Dictionary<string, Stock>(StringComparer.OrdinalIgnoreCase);

			private readonly List<Transaction> _transactions = new List<Transaction>();
			private readonly Dictionary<(Account fromAccount, Account toAccount, DateTime date, decimal amount), Transaction> _transfers =
				new Dictionary<(Account fromAccount, Account toAccount, DateTime date, decimal amount), Transaction>(new TransactionComparer());

			private class TransactionComparer : IEqualityComparer<(Account fromAccount, Account toAccount, DateTime date, decimal amount)>
			{
				public bool Equals([AllowNull] (Account fromAccount, Account toAccount, DateTime date, decimal amount) x, [AllowNull] (Account fromAccount, Account toAccount, DateTime date, decimal amount) y) =>
					(x.fromAccount.Name == y.fromAccount.Name)
					&& (x.toAccount.Name == y.toAccount.Name)
					&& (x.date == y.date)
					&& (x.amount == y.amount);

				public int GetHashCode([DisallowNull] (Account fromAccount, Account toAccount, DateTime date, decimal amount) obj) =>
					HashCode.Combine(obj.fromAccount.Name, obj.toAccount, obj.date, obj.amount);
			}

			public async Task<Qif> Parse()
			{
				await GetNextRecord();

				while (true)
				{
					if (_record == null)
						break;

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

				return _qif;
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
					_currentAccount = _accounts.GetOrAdd(
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

			private async Task ParseNonInvestmentTransactions()
			{
				_record.RemoveAt(0);

				while (true)
				{
					if (_record[0][0] == '!')
						return;

					var dict = _record.TakeWhile(s => s[0] != 'S').ToDictionary(x => x[0], x => x[1..]);
					var date = DateTime.ParseExact(dict['D'], new[] { @"M\/dd\'yy", @"M\/ d\'yy", @"M\/dd\' y", @"M\/ d\' y", }, null, System.Globalization.DateTimeStyles.None);

					var amount = Convert.ToDecimal(dict['T']);

					var payor = dict.GetOrDefault('P', string.Empty);
					var memo = dict.GetOrDefault('M');
					var cleared = dict.GetOrDefault('C') switch
					{
						"c" => ClearedStatus.Cleared,
						"R" => ClearedStatus.Reconciled,
						_ => ClearedStatus.None,
					};

					var transaction = new Transaction
					{
						Date = date,
						Description = payor,
						Memo = memo,
						TransactionItems =
						{
							new TransactionItem
							{
								Account = GetCurrentAccount(),
								StockId = (StockId)0,
								Amount = amount,
								Shares = amount,
								ClearedStatus = cleared,
								Memo = memo,
							},
						}
					};

					// assumptions. awful file format. kill me now.
					var splits = _record
						.SkipWhile(s => s[0] != 'S')
						.TakeWhile(s => s[0] != '^')
						.Segment(s => s[0] == 'S')
						.Select(arr => arr.ToDictionary(s => s[0], s => s.Length == 1 ? "Miscellaneous" : s[1..]))
						.Select(d => (account: d['S'], amount: -Convert.ToDecimal(d['$']), memo: d.GetOrDefault('E')))
						.ToArray();

					// my own terrible record keeping?
					var destination = dict.GetOrDefault('L', "Miscellaneous");
					if (!splits.Any())
						splits = new[] { (account: destination, amount: -amount, memo: default(string)), };

					transaction.TransactionItems.AddRange(
						splits
							.Select(s => new TransactionItem
							{
								Account = _accounts[s.account[0] == '[' ? s.account[1..^1] : s.account],
								StockId = (StockId)0,
								Amount = s.amount,
								Shares = s.amount,
								Memo = s.memo,
							}));

					if (transaction.Balance != 0)
						throw new InvalidOperationException("Transaction out of balance!!");

					var transferSplits = splits.Where(s => s.account[0] == '[').Select(s => s.account[1..^1]).ToArray();
					if (transferSplits.Any())
					{
						if (transferSplits.Any(s => s == GetCurrentAccount().Name))
							throw new NotImplementedException();

						var existingTransfers = transferSplits
							.Select(s => _transfers.GetOrDefault((_accounts[s], GetCurrentAccount(), date, Math.Abs(amount))))
							.Where(t => t != null)
							.ToArray();
						if (existingTransfers.Length == 1 && splits.Length == 1)
						{
							existingTransfers[0]!.TransactionItems
								.FirstOrDefault(ti => ReferenceEquals(ti.Account, GetCurrentAccount()))
								.ClearedStatus = cleared;
						}
						else
						{
							foreach (var t in existingTransfers)
							{
								_transactions.Remove(t!);

								var existingItem = t!.TransactionItems[0];
								var account = existingItem.Account;

								foreach (var ti in transaction.TransactionItems.Where(ti => ReferenceEquals(ti.Account, account)))
									ti.ClearedStatus = existingItem.ClearedStatus;
							}

							_transactions.Add(transaction);

							foreach (var s in splits.Where(s => s.account[0] == '['))
								_transfers[(GetCurrentAccount(), _accounts[s.account[1..^1]], date, Math.Abs(s.amount))] = transaction;
						}
					}
					else
						_transactions.Add(transaction);

					await GetNextRecord();
				}
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
					_stocks.GetOrAdd(
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
