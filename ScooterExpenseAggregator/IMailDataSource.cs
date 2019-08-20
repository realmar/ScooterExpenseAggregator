using System.Collections.Generic;
using System.Threading.Tasks;

namespace Realmar.ScooterExpenseAggregator
{
    public interface IMailDataSource
    {
        IAsyncEnumerable<string> EnumerateMailsAsync(string address);
    }
}