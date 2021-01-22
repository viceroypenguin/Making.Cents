using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DryIoc;
using Going.Plaid;
using LinqToDB;
using LinqToDB.Data;
using Making.Cents.Data;
using Making.Cents.Data.Services;
using Making.Cents.ViewModels;
using Making.Cents.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Making.Cents
{
	internal static class Bootstrapper
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		public static void Run(string[] args)
		{
			var container = new Container(
				rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments),
				scopeContext: new AsyncExecutionFlowScopeContext());
			container.RegisterInstanceMany(BuildConfiguration());

			container.InitializeLogging();

			var logger = container.Resolve<ILoggerFactory>().CreateLogger(typeof(Bootstrapper));
			logger.LogDebug("Logging initialized");

			container.RegisterConnectionString(args);

			container.RegisterOptions();
			container.RegisterDataSources();
			container.RegisterServices();
			container.RegisterViewModels();
			logger.LogDebug("DryIoC initialized");

			InitializeDatabase(container);
			logger.LogDebug("Database initialized");

#if true
			if (false)
			{
				var qif = Task.Run(() => Qif.Qif.ReadFile(@"C:\Users\stuar\OneDrive\Documents\Finances\my money.qif", container.Resolve<ILogger<Qif.Qif>>()))
					.GetAwaiter()
					.GetResult();
				using (var context = container.Resolve<DbContext>())
				{
					foreach (var security in qif.Stocks.Where(s => s.Name != "CASH"))
					{
						security.SecurityId =
							context.Securities
								.InsertWithOutput(new Data.Models.Security
								{
									Name = security.Name,
									Ticker = security.Ticker,
								})
								.SecurityId;
					}

					var tickerMap = qif.Stocks
						.ToDictionary(x => x.Ticker, x => x.SecurityId);

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
								SecurityId = ti.Security?.SecurityId ?? ti.SecurityId,
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

			InitializeSystem(container);

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

		private static void InitializeLogging(this Container container)
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

		private static void RegisterConnectionString(this Container container, string[] args)
		{
			var rootCommand = new RootCommand("A WPF Accounting Software.")
			{
				new Option<string?>(
					"--connection-string-name",
					getDefaultValue: () => container.GetDefaultConnectionString(),
					description: "The connection string name to use for data storage."),
			};

			rootCommand.Handler = CommandHandler.Create<string?>(
				connectionStringName =>
				{
					var connectionString = container.Resolve<IConfigurationRoot>().GetConnectionString(
						connectionStringName ?? throw new ArgumentNullException(nameof(connectionStringName)));
					container.RegisterInstance(new DbContextOptions { ConnectionString = connectionString, });
				});

			rootCommand.Invoke(args);
		}

		private static string? GetDefaultConnectionString(this Container container)
		{
			var configuration = container.Resolve<IConfigurationRoot>();
			return configuration.GetValue<string?>("DefaultConnectionString");
		}

		private static void RegisterOptions(this Container container)
		{
			container.Register(typeof(IOptionsFactory<>), typeof(OptionsFactory<>), Reuse.Transient);
			container.Register(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>), Reuse.Singleton);
			container.Register(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>), Reuse.Singleton);
			container.Register(typeof(IOptions<>), typeof(OptionsManager<>), Reuse.Singleton);
			container.Register(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>), Reuse.Scoped);
		}

		private static void RegisterDataSources(this Container container)
		{
			container.Register<DbContext>(Reuse.Transient, setup: Setup.With(allowDisposableTransient: true));

			var config = container.Resolve<IConfigurationRoot>().GetSection("Plaid");
			container.Configure<Making.Cents.PlaidModule.Models.PlaidOptions>(config);
			container.Configure<Going.Plaid.PlaidOptions>(config);
			container.Register<PlaidClient>(Reuse.Singleton);
		}

		private static void Configure<T>(this Container container, IConfiguration config)
			where T : class
		{
			var name = Options.DefaultName;
			container.RegisterInstance<IOptionsChangeTokenSource<T>>(new ConfigurationChangeTokenSource<T>(name, config));
			container.RegisterInstance<IConfigureOptions<T>>(new NamedConfigureFromConfigurationOptions<T>(name, config));
		}

		private static void RegisterServices(this Container container)
		{
			container.Register<AccountService>(Reuse.Singleton);
			container.Register<SecurityService>(Reuse.Singleton);
			container.Register<TransactionService>(Reuse.Singleton);
		}

		private static void RegisterViewModels(this Container container)
		{
			container.RegisterAccountModule();
			container.RegisterPlaidModule();

			container.RegisterMany<ShellViewModel>(Reuse.Singleton);
		}

		private static void InitializeDatabase(Container container)
		{
			using (var context = container.Resolve<DbContext>())
				context.InitializeDatabase();
		}

		private static void InitializeSystem(Container container) => 
			Task.Run(async () =>
			{
				await container.Resolve<AccountService>().InitializeAsync();
				await container.Resolve<SecurityService>().InitializeAsync();
				await container.Resolve<TransactionService>().InitializeAsync();
			}).Wait();
	}
}
