using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

		public IReadOnlyList<Security> Stocks { get; private set; } = null!;
		public IReadOnlyList<Account> Accounts { get; private set; } = null!;
		public IReadOnlyList<Transaction> Transactions { get; private set; } = null!;
		public IReadOnlyList<Security> StockValues { get; private set; } = null!;

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
			private int _recordNumber = 0;

			private readonly Dictionary<string, Account> _accounts =
				new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase)
				{
					["Initial Equity"] = new Account { Name = "Initial Equity", AccountType = AccountType.Income, AccountSubType = AccountSubType.Income, },
				};

			private readonly Dictionary<string, Security> _stocks =
				new Dictionary<string, Security>(StringComparer.OrdinalIgnoreCase)
				{
					["CASH"] = new Security { StockId = (SecurityId)0, Name = "CASH", },
				};

			private readonly List<Security> _prices = new List<Security>();

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
				try
				{
					await GetNextRecord();

					while (true)
					{
						if (_record == null)
							break;

						if (!_record.Any())
							break;

						switch (_record[0])
						{
							case "!Account": await ParseAccounts(); break;
							case "!Type:":  // wtf???
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
				catch (Exception e)
				{
					_logger.LogError(e, "Parsing failed on line {line}", _lineNumber);
					throw new InvalidOperationException($"Parsing failed on line {_lineNumber}", e);
				}

				_qif.Stocks = _stocks.Values.ToArray();
				_qif.Accounts = _accounts.Values.ToArray();
				_qif.Transactions = _transactions;
				_qif.StockValues = _prices;
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
					{
						_logger?.LogDebug("Record #{recordNumber} parsed.", ++_recordNumber);
						return;
					}

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
					"Port" => (AccountType.Asset, AccountSubType.Brokerage), // will have to differentiate later
					"Oth A" => (AccountType.Asset, AccountSubType.OtherAsset),
					"401(k)/403(b)" => (AccountType.Asset, AccountSubType.Retirement),
					"IRA" => (AccountType.Asset, AccountSubType.Retirement),
					"Hsa" => (AccountType.Asset, AccountSubType.Hsa),
					"CCard" => (AccountType.Liability, AccountSubType.CreditCard),
					"Oth L" => (AccountType.Liability, AccountSubType.OtherLiability),
					"" => (AccountType.Liability, AccountSubType.OtherLiability), // WTF?
					_ => throw new InvalidOperationException($"Unknown account type: '{detail}'."),
				};

			private async Task ParseInvestmentTransactions()
			{
				_record.RemoveAt(0);

				var inventory = new Dictionary<string, List<(decimal shares, decimal value)>>();

				while (true)
				{
					if (_record[0][0] == '!')
						return;

					var dict = _record.TakeWhile(s => s[0] != 'S').ToDictionary(x => x[0], x => x[1..]);
					var date = ParseDate(dict['D']);

					var cashAmount = Convert.ToDecimal(dict.GetOrDefault('T', "0.00"));

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
								StockId = (SecurityId)0,
								ClearedStatus = cleared,
								Memo = memo,
							},
						}
					};

					switch (dict['N'])
					{
						case "WithdrwX":
						case "XOut":
							cashAmount = -cashAmount;
							goto case "XIn";

						case "Cash":
						case "XIn":
						case "ContribX":
						{
							transaction.TransactionItems[0].Amount = cashAmount;
							transaction.TransactionItems[0].Shares = cashAmount;

							// my own terrible record keeping?
							var destination = dict.GetOrDefault('L', "Miscellaneous");
							HandleCashTransaction(
								date,
								cashAmount,
								cleared,
								transaction,
								new[]
								{
									(destination, -cashAmount, default(string?)),
								});
							break;
						}

						case "Buy" when memo?.Contains("NAME CHANGED") ?? false:
						{
							var newStock = dict['Y'];
							var shares = Convert.ToDecimal(dict['Q']);

							await GetNextRecord();

							dict = _record.TakeWhile(s => s[0] != 'S').ToDictionary(x => x[0], x => x[1..]);

							var oldStock = dict['Y'];

							if (inventory[oldStock].Sum(s => s.shares) != shares)
								throw new InvalidOperationException("Number of shares doesn't match...");

							cashAmount = inventory[oldStock].Sum(s => s.value * s.shares);

							inventory[newStock] = inventory[oldStock];
							inventory.Remove(oldStock);

							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[newStock],
									Amount = cashAmount,
									Shares = shares,
								});
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[oldStock],
									Amount = -cashAmount,
									Shares = -shares,
								});

							_transactions.Add(transaction);

							break;
						}

						case "Buy":
						{
							transaction.TransactionItems[0].Amount = -cashAmount;
							transaction.TransactionItems[0].Shares = -cashAmount;

							var ti = new TransactionItem
							{
								Account = GetCurrentAccount(),
								Stock = _stocks[dict['Y']],
								Amount = cashAmount,
								Shares = Convert.ToDecimal(dict['Q']),
							};
							transaction.TransactionItems.Add(ti);

							inventory.GetOrAdd(dict['Y'], new List<(decimal shares, decimal value)>())
								.Add((ti.Shares, ti.PerShare));

							if (dict.TryGetValue('O', out var commission))
							{
								var amt = Convert.ToDecimal(commission);
								transaction.TransactionItems[1].Amount -= amt;

								transaction.TransactionItems.Add(
									new TransactionItem
									{
										Account = _accounts["Fees"],
										StockId = (SecurityId)0,
										Amount = amt,
										Shares = amt,
									});
							}

							if (Math.Abs(transaction.TransactionItems[1].PerShare - Convert.ToDecimal(dict['I'])) > 0.05m)
								throw new InvalidOperationException();

							_transactions.Add(transaction);
							break;
						}

						case "Sell":
						{
							transaction.TransactionItems[0].Amount = cashAmount;
							transaction.TransactionItems[0].Shares = cashAmount;

							var stock = dict['Y'];
							var shares = Convert.ToDecimal(dict['Q']);
							var baseValue = 0m;
							var holdings = inventory[stock];

							while (shares > holdings[0].shares)
							{
								baseValue += holdings[0].shares * holdings[0].value;
								shares -= holdings[0].shares;
								holdings.RemoveAt(0);
							}

							baseValue = Math.Round(baseValue + shares * holdings[0].value, 2);
							if (holdings[0].shares == shares)
								holdings.RemoveAt(0);
							else
								holdings[0] = (holdings[0].shares - shares, holdings[0].value);

							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[stock],
									Amount = -baseValue,
									Shares = -Convert.ToDecimal(dict['Q']),
								});

							var gains = cashAmount - baseValue;
							if (dict.TryGetValue('O', out var commission))
							{
								var amt = Convert.ToDecimal(commission);
								gains += amt;

								transaction.TransactionItems.Add(
									new TransactionItem
									{
										Account = _accounts["Fees"],
										StockId = (SecurityId)0,
										Amount = amt,
										Shares = amt,
									});
							}

							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = _accounts["Investment Income:Capital Gains"],
									StockId = (SecurityId)0,
									Amount = -gains,
									Shares = -gains,
								});

							_transactions.Add(transaction);
							break;
						}

						case "SellX":
						{
							var destination = dict['L'][1..^1];
							var account = _accounts[destination];

							if (_transfers.TryGetValue((account, GetCurrentAccount(), date, Math.Abs(cashAmount)), out var tfrTransaction))
							{
								tfrTransaction.TransactionItems[1].Stock = _stocks[dict['Y']];
								tfrTransaction.TransactionItems[1].Shares = -Convert.ToDecimal(dict['Q']);

								if (Math.Abs(tfrTransaction.TransactionItems[1].PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
									throw new InvalidOperationException();
								break;
							}
							else
							{
								transaction.TransactionItems[0].Account = account;
								_transfers[(GetCurrentAccount(), account, date, Math.Abs(cashAmount))] = transaction;
								goto case "Sell";
							}
						}

						case "RtrnCap":
						case "Div":
							if (string.IsNullOrWhiteSpace(transaction.Description))
								transaction.Description = "Dividend Distribution";

							transaction.TransactionItems[0].Amount = cashAmount;
							transaction.TransactionItems[0].Shares = cashAmount;

							// so we know which stock delivered the dividend
							if (dict.ContainsKey('Y'))
								transaction.TransactionItems.Add(
									new TransactionItem
									{
										Account = GetCurrentAccount(),
										Stock = _stocks[dict['Y']],
										Amount = 0,
										Shares = 0,
									});

							// dividend income
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = _accounts["Investment Income:Dividends"],
									StockId = (SecurityId)0,
									Amount = -cashAmount,
									Shares = -cashAmount,
								});

							_transactions.Add(transaction);
							break;

						case "ReinvSh":
						case "ReinvLg":
						case "ReinvDiv":
						{
							if (string.IsNullOrWhiteSpace(transaction.Description))
								transaction.Description = "Reinvest Dividend";

							// stock repurchase
							var ti = new TransactionItem
							{
								Account = GetCurrentAccount(),
								Stock = _stocks[dict['Y']],
								Amount = cashAmount,
								Shares = Convert.ToDecimal(dict['Q']),
							};
							transaction.TransactionItems.Add(ti);

							inventory.GetOrAdd(dict['Y'], new List<(decimal shares, decimal value)>())
								.Add((ti.Shares, ti.PerShare));

							if (Math.Abs(ti.PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
								throw new InvalidOperationException();

							// dividend income
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = _accounts["Investment Income:Dividends"],
									StockId = (SecurityId)0,
									Amount = -cashAmount,
									Shares = -cashAmount,
								});

							_transactions.Add(transaction);
							break;
						}

						case "ShrsOut":
							break;

						case "ShrsIn":
						{
							if (memo == "0 shares added to account")
								break;

							var stock = _stocks[dict['Y']];
							var qty = Convert.ToDecimal(dict['Q']);

							transaction.TransactionItems.Clear();
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = stock,
									Amount = cashAmount,
									Shares = qty,
								});

							if (memo == null)
								transaction.TransactionItems.Add(
									new TransactionItem
									{
										Account = _accounts["Initial Equity"],
										StockId = (SecurityId)0,
										Amount = -cashAmount,
										Shares = -cashAmount,
									});
							else
								transaction.TransactionItems.Add(
									new TransactionItem
									{
										Account = _accounts[memo.Replace("Xfr From: ", "")],
										Stock = stock,
										Amount = -cashAmount,
										Shares = -qty,
									});

							var ti = transaction.TransactionItems[0];
							inventory.GetOrAdd(dict['Y'], new List<(decimal shares, decimal value)>())
								.Add((ti.Shares, ti.PerShare));

							if (Math.Abs(transaction.TransactionItems[0].PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
								throw new InvalidOperationException();
							if (memo != null
								&& Math.Abs(transaction.TransactionItems[1].PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
								throw new InvalidOperationException();

							_transactions.Add(transaction);
							break;
						}

						case "StkSplit":
						{
							if (string.IsNullOrWhiteSpace(transaction.Description))
								transaction.Description = "Stock Split";

							var stock = dict['Y'];
							var holdings = inventory[stock];

							// fuck quicken. who the hell decided that 
							// Quantity should be the multiplier instead of
							// an actual quantity. and 10* that no less
							var factor = Convert.ToDecimal(dict['Q']) / 10m;

							inventory[stock] = holdings
								.Select(s => (Math.Round(s.shares * factor, 4), s.value / factor))
								.ToList();

							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[stock],
									Shares = inventory[stock].Sum(s => s.shares) - holdings.Sum(s => s.shares),
								});

							_transactions.Add(transaction);
							break;
						}

						case "Rename":
						{
							if (string.IsNullOrWhiteSpace(transaction.Description))
								transaction.Description = "Stock Rename";

							var qty = Convert.ToDecimal(dict['Q']);
							transaction.TransactionItems.Clear();
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[dict['Y']],
									Amount = -cashAmount,
									Shares = -qty,
								});
							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = GetCurrentAccount(),
									Stock = _stocks[dict['Z']],
									Amount = cashAmount,
									Shares = qty,
								});

							if (Math.Abs(transaction.TransactionItems[0].PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
								throw new InvalidOperationException();
							if (Math.Abs(transaction.TransactionItems[1].PerShare - Convert.ToDecimal(dict['I'])) > 0.01m)
								throw new InvalidOperationException();

							_transactions.Add(transaction);
							break;
						}

						case "IntInc":
							transaction.TransactionItems[0].Amount = cashAmount;
							transaction.TransactionItems[0].Shares = cashAmount;

							transaction.TransactionItems.Add(
								new TransactionItem
								{
									Account = _accounts["Investment Income:Interest"],
									StockId = (SecurityId)0,
									Amount = -cashAmount,
									Shares = -cashAmount,
								});

							_transactions.Add(transaction);
							break;

						default:
							throw new NotImplementedException();
					}

					if (transaction.Balance != 0)
						throw new InvalidOperationException("Transaction out of balance!!");

					await GetNextRecord();
				}
			}

			private async Task ParseNonInvestmentTransactions()
			{
				_record.RemoveAt(0);

				while (true)
				{
					if (_record[0][0] == '!')
						return;

					var dict = _record.TakeWhile(s => s[0] != 'S').ToDictionary(x => x[0], x => x[1..]);
					var date = ParseDate(dict['D']);

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
								StockId = (SecurityId)0,
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
						splits = new[] { (account: destination, amount: -amount, memo: default(string?)), };

					HandleCashTransaction(date, amount, cleared, transaction, splits);

					if (transaction.Balance != 0)
						throw new InvalidOperationException("Transaction out of balance!!");

					await GetNextRecord();
				}
			}

			private static DateTime ParseDate(string dateStr) =>
				DateTime.ParseExact(
					dateStr,
					new[]
					{
						@"M\/dd\'yy",
						@"M\/ d\'yy",
						@"M\/dd\' y",
						@"M\/ d\' y",
						@" M\/dd\'yy",
						@" M\/ d\'yy",
						@" M\/dd\' y",
						@" M\/ d\' y",
						@" M\/ d\/yy",
					},
					null,
					System.Globalization.DateTimeStyles.None);

			private void HandleCashTransaction(DateTime date, decimal amount, ClearedStatus cleared, Transaction transaction, (string account, decimal amount, string? memo)[] splits)
			{
				transaction.TransactionItems.AddRange(
					splits
						.Select(s => new TransactionItem
						{
							Account = _accounts[s.account[0] == '[' ? s.account[1..^1] : s.account],
							StockId = (SecurityId)0,
							Amount = s.amount,
							Shares = s.amount,
							Memo = s.memo,
						}));

				var transferSplits = splits.Where(s => s.account[0] == '[').Select(s => (account: s.account[1..^1], amount: s.amount)).ToArray();
				if (transferSplits.Any())
				{
					if (transferSplits.Any(s => s.account == GetCurrentAccount().Name))
					{
						if (transaction.Description == "Opening Balance")
						{
							transaction.TransactionItems[^1].Account = _accounts["Initial Equity"];
							_transactions.Add(transaction);
						}
						else
							throw new NotImplementedException();
					}

					var existingTransfers = transferSplits
						.Select(s => _transfers.GetOrDefault((_accounts[s.account], GetCurrentAccount(), date, Math.Abs(s.amount))))
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
			}

			private Task ParseClassifications()
			{
				throw new NotImplementedException();
			}

			private Task ParseMemorizedTransactions()
			{
				throw new NotImplementedException();
			}

			private async Task ParseSecurityPrice()
			{
				var data = _record[1].Split(',');
				if (!string.IsNullOrWhiteSpace(data[1]))
					_prices.Add(
						new Security
						{
							Ticker = data[0].Trim('"'),
							CurrentValueDate = ParseDate(data[2].Trim('"')),
							CurrentValue = ConvertPrice(data[1]),
						});

				await GetNextRecord();
			}

			private decimal ConvertPrice(string v)
			{
				if (!v.Contains('/'))
					return Convert.ToDecimal(v);

				var split = v.Split();
				var whole = Convert.ToDecimal(split[0]);
				split = split[1].Split('/');
				var fraction = Convert.ToDecimal(split[0]) / Convert.ToDecimal(split[1]);
				return whole + fraction;
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
						_ => new Security
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
