using System.Collections.Generic;
using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    public interface IScooterCompany
    {
        string Name { get; }
        IAsyncEnumerable<ScooterRide> EnumerateRidesAsync();
    }
}