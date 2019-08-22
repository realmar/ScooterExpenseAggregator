using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Realmar.ScooterExpenseAggregator
{
    internal class Program
    {
        private static Logger _logger;

        private static async Task Main()
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
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static async Task RunApplication()
        {
            var dataSource = await MailSourceFactory.Create(GetMailSourceName()).ConfigureAwait(false);
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

        private static string GetMailSourceName()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration["Source"];
        }
    }
}