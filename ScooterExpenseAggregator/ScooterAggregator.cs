using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    public class ScooterAggregator
    {
        private readonly Dictionary<string, ScooterRide> _rides = new Dictionary<string, ScooterRide>();
        private readonly IReadOnlyCollection<IScooterCompany> _companies;

        public double TotalCost => _rides.Values.Select(ride => ride.Price).Sum();
        public double TotalDistance => _rides.Values.Select(ride => ride.Distance).Sum();

        public ScooterAggregator(IReadOnlyCollection<IScooterCompany> companies)
        {
            _companies = companies;
        }

        public async Task AggregateAsync()
        {
            _rides.Clear();
            foreach (var company in _companies)
            {
                var aggregate = new ScooterRide();
                await foreach (var ride in company.EnumerateRidesAsync())
                {
                    aggregate += ride;
                }

                _rides[company.Name] = aggregate;
            }
        }
    }
}