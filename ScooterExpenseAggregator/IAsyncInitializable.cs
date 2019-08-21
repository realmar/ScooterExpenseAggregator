using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}