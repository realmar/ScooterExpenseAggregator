using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    internal class Program
    {
        private static async Task Main()
        {
            var dataSource = await CreateMailDataSourceAsync().ConfigureAwait(false);
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

        private static async Task<IMailDataSource> CreateMailDataSourceAsync()
        {
            var settings = ConfigurationManager.AppSettings;
            var sourceName = settings["Source"];

            var source = sourceName switch
            {
                "outlook" => (IAsyncInitializable) new OutlookMailSource(),
                "gmail" => new GmailMailSource(),
                _ => throw new ArgumentException($"Cannot find provider for {sourceName}")
            };

            await source.InitializeAsync().ConfigureAwait(false);

            return (IMailDataSource) source;
        }
    }
}