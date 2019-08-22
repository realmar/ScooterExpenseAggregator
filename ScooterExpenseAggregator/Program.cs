using System;
using System.Configuration;
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
            var dataSource = await MailSourceFactory.Create(sourceName).ConfigureAwait(false);
            var companies = new IScooterCompany[]
            {
                new CircCompany(dataSource),
                new TierCompany(dataSource)
            };

            var aggregator = new ScooterAggregator(companies);
            await aggregator.AggregateAsync().ConfigureAwait(false);

            LogManager.Flush();

            Console.WriteLine();
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
    }
}