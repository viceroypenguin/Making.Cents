using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DryIoc;
using EnumsNET;
using Going.Plaid;
using LinqToDB;
using LinqToDB.Data;
using Making.Cents.AccountsModule.ViewModels;
using Making.Cents.Data;
using Making.Cents.Data.Services;
using Making.Cents.ViewModels;
using Making.Cents.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Making.Cents
{
	internal class Bootstrapper
	{

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		public static void Run()
		{
			var container = new Container();
			container.RegisterInstance(BuildConfiguration());

			InitializeLogging(container);

			var logger = container.Resolve<ILogger<Bootstrapper>>();
			logger.LogDebug("Logging initialized");

			RegisterDataSources(container);
			RegisterServices(container);
			RegisterViewModels(container);
			logger.LogDebug("DryIoC initialized");

			InitializeDatabase(container);
			logger.LogDebug("Database initialized");

#if true
			if (true)
			{
				var qif = Task.Run(() => Qif.Qif.ReadFile(@"C:\Users\stuar\OneDrive\Documents\Finances\my money.qif", container.Resolve<ILogger<Qif.Qif>>()))
					.GetAwaiter()
					.GetResult();
				using (var context = container.Resolve<DbContext>())
				{
					foreach (var security in qif.Stocks)
					{
						security.StockId =
							context.Securities
								.InsertWithOutput(new Data.Models.Security
								{
									Name = security.Name,
									Ticker = security.Ticker,
								})
								.SecurityId;
					}

					var tickerMap = qif.Stocks
						.ToDictionary(x => x.Ticker, x => x.StockId);

					foreach (var account in qif.Accounts)
					{
						account.AccountId =
							context.Accounts
								.InsertWithOutput(new Data.Models.Account
								{
									Name = account.Name,
									AccountTypeId = account.AccountType,
									AccountSubTypeId = account.AccountSubType,
								})
								.AccountId;
					}

					foreach (var transaction in qif.Transactions)
					{
						transaction.TransactionId =
							context.Transactions
								.InsertWithOutput(new Data.Models.Transaction
								{
									Date = transaction.Date,
									Description = transaction.Description,
									Memo = transaction.Memo,
								})
								.TransactionId;
					}

					context.BulkCopy(qif.Transactions
						.SelectMany(t => t.TransactionItems
							.Select(ti => new Data.Models.TransactionItem
							{
								TransactionId = t.TransactionId,
								AccountId = ti.Account!.AccountId,
								SecurityId = ti.Stock?.StockId ?? ti.StockId,
								Amount = ti.Amount,
								Shares = ti.Shares,
								Memo = ti.Memo,
								ClearedStatusId = ti.ClearedStatus,
							})));

					context.BulkCopy(qif.StockValues
						.Select(sv => new Data.Models.SecurityValue
						{
							SecurityId = tickerMap[sv.Ticker],
							Date = sv.CurrentValueDate,
							Value = sv.CurrentValue,
						}));
				}
			}
#endif

			var vm = container.Resolve<ShellViewModel>();

			var window =
				new ShellView
				{
					DataContext = vm,
				};
			window.Show();

			_ = vm.InitializeAsync();
			logger.LogDebug("Application started");
		}

		private static void InitializeLogging(Container container)
		{
			AllocConsole();

			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Debug()
				.WriteTo.Console(
					outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}",
					theme: AnsiConsoleTheme.Code)
				.CreateLogger();

			var factory = new Serilog.Extensions.Logging.SerilogLoggerFactory();

			container.RegisterInstance<ILoggerFactory>(factory);
			container.Register(
				Made.Of(() => factory.CreateLogger(null)),
				setup: Setup.With(condition: r => r.Parent.ImplementationType == null));
			container.Register(
				Made.Of(
					() => factory.CreateLogger(Arg.Index<Type>(0)),
					r => r.Parent.ImplementationType),
				setup: Setup.With(condition: r => r.Parent.ImplementationType != null));

			var method = typeof(LoggerFactoryExtensions)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(m => m.Name == nameof(LoggerFactoryExtensions.CreateLogger))
				.Where(m => m.ContainsGenericParameters)
				.Single();

			container.Register(
				typeof(ILogger<>),
				made: Made.Of(method));
		}

		private static IConfigurationRoot BuildConfiguration() =>
			new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.secrets.json", optional: true)
				.Build();

		private static void RegisterDataSources(Container container)
		{
			container.Register<DbContext>(Reuse.Transient, setup: Setup.With(allowDisposableTransient: true));

			var configuration = container.Resolve<IConfigurationRoot>().GetSection("Plaid");

			var environment = Enums.Parse<Going.Plaid.Environment>(configuration["environment"]);
			var clientId = configuration["clientId"];
			var secret = configuration["secret"];
			container.RegisterInstance(
				new PlaidClient(
					environment: environment,
					clientId: clientId,
					secret: secret));

			foreach (var c in configuration.GetSection("accessTokens").GetChildren())
			{
			}
		}

		private static void RegisterServices(Container container)
		{
			container.Register<AccountService>(Reuse.Singleton);
		}

		private static void RegisterViewModels(Container container)
		{
			container.Register<ShellViewModel>(Reuse.Singleton);
			container.Register<PlaidAccountsViewModel>(Reuse.Singleton);
		}

		private static void InitializeDatabase(Container container)
		{
			using (var context = container.Resolve<DbContext>())
				context.InitializeDatabase();
		}
	}
}
