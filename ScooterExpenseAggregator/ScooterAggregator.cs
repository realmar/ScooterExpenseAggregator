using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    public class ScooterAggregator
    {
        private readonly Dictionary<IScooterCompany, ScooterRide> _rides =
            new Dictionary<IScooterCompany, ScooterRide>();

        private readonly IReadOnlyCollection<IScooterCompany> _companies;

        public int TotalRides { get; private set; }
        public double TotalCost => _rides.Values.Select(ride => ride.Price).Sum();
        public double TotalDistance => _rides.Values.Select(ride => ride.Distance).Sum();
        public double TotalTime => _rides.Values.Select(ride => ride.Time).Sum();

        public ScooterAggregator(IReadOnlyCollection<IScooterCompany> companies)
        {
            _companies = companies;
        }

        public async Task AggregateAsync()
        {
            TotalRides = 0;
            _rides.Clear();

            foreach (var company in _companies)
            {
                var aggregate = new ScooterRide();
                await foreach (var ride in company.EnumerateRidesAsync())
                {
                    TotalRides++;
                    aggregate += ride;
                }

                _rides[company] = aggregate;
            }
        }

        public IEnumerable<(IScooterCompany Company, ScooterRide Ride)> EnumerateByCompany()
        {
            foreach (var (company, ride) in _rides)
            {
                yield return (company, ride);
            }
        }
    }
}