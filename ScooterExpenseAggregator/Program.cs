using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace Realmar.ScooterExpenseAggregator
{
    internal class Program
    {
        private static Logger _logger;

        private static async Task Main()
        {
            try
            {
                LogManager.LoadConfiguration("NLog.config");
                _logger = LogManager.GetCurrentClassLogger();

                try
                {
                    await RunApplication().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    throw;
                }
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static async Task RunApplication()
        {
            var settings = ConfigurationManager.AppSettings;
            var sourceName = settings["Source"];
            var dataSource = await CreateMailDataSourceAsync(sourceName).ConfigureAwait(false);
            var companies = new IScooterCompany[]
            {
                new CircCompany(dataSource),
                new TierCompany(dataSource)
            };

            var aggregator = new ScooterAggregator(companies);
            await aggregator.AggregateAsync().ConfigureAwait(false);

            Console.WriteLine($"Rides: {aggregator.TotalRides}");
            Console.WriteLine($"Cost: {aggregator.TotalCost} " +
                              $"Distance: {aggregator.TotalDistance} " +
                              $"Time: {aggregator.TotalTime}");

            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine();

            foreach (var (company, ride) in aggregator.EnumerateByCompany())
            {
                Console.WriteLine(company.Name);
                Console.WriteLine(ride.ToString());
                Console.WriteLine();
            }
        }

        private static async Task<IMailDataSource> CreateMailDataSourceAsync(string sourceName)
        {
            var sourceType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IMailDataSource).IsAssignableFrom(type))
                .FirstOrDefault(type => type.Name.StartsWith(sourceName, StringComparison.InvariantCultureIgnoreCase));

            if (sourceType == null)
            {
                throw new ArgumentException($"Cannot find mail data provider with name {sourceName}", sourceName);
            }

            _logger.Info($"Creating mail data source {sourceName}");
            var source = Activator.CreateInstance(sourceType);
            if (source is IAsyncInitializable initializable)
            {
                _logger.Debug("Initializing mail data source");
                await initializable.InitializeAsync().ConfigureAwait(false);
                _logger.Debug("Mail data source initialized");
            }

            return (IMailDataSource) source;
        }
    }
}