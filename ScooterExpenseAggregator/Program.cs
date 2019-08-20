using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Realmar.ScooterExpenseAggregator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var dataSource = await CreateMailDataSourceAsync().ConfigureAwait(false);
            var companies = new IScooterCompany[]
            {
                new CircCompany(dataSource),
                new TierCompany(dataSource)
            };

            var rideCounter = 0;
            var aggregatedRide = new ScooterRide();

            foreach (var company in companies)
            {
                await foreach (var ride in company.EnumerateRidesAsync())
                {
                    rideCounter++;
                    aggregatedRide += ride;
                }
            }

            Console.WriteLine($"Ride Count: {rideCounter}");
            Console.WriteLine(aggregatedRide.ToString());
        }

        private static async Task<IMailDataSource> CreateMailDataSourceAsync()
        {
            var settings = ConfigurationManager.AppSettings;
            var sourceName = settings["Source"];
            IMailDataSource source = null;

            switch (sourceName)
            {
                case "outlook":
                    var outlook = new OutlookMailSource();
                    await outlook.InitializeAsync().ConfigureAwait(false);
                    source = outlook;
                    break;
                case "gmail":
                    var gmail = new GmailMailSource();
                    await gmail.InitializeAsync().ConfigureAwait(false);
                    source = gmail;
                    break;
                default:
                    throw new ArgumentException($"Cannot find provider for {sourceName}");
            }

            return source;
        }
    }
}