using System;
using System.Collections.Generic;
using ScooterExpenseAggregator.Properties;

namespace Realmar.ScooterExpenseAggregator
{
    public class MockMailDataSource : IMailDataSource
    {
        public async IAsyncEnumerable<string> EnumerateMailsAsync(string address)
        {
            yield return default;
        }
    }
}