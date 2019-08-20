using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Realmar.ScooterExpenseAggregator.DataHelper;

namespace Realmar.ScooterExpenseAggregator
{
    public class CircCompany : IScooterCompany
    {
        private readonly IMailDataSource _mailDataSource;

        public CircCompany(IMailDataSource mailDataSource)
        {
            _mailDataSource = mailDataSource;
        }

        public string Name => "Circ";

        public async IAsyncEnumerable<ScooterRide> EnumerateRidesAsync()
        {
            var mails = _mailDataSource.EnumerateMailsAsync("billing@circ.com").ConfigureAwait(false);
            await foreach (var mail in mails)
            {
                var result = new ScooterRide();
                try
                {
                    int end;
                    double distance;
                    double minutes;
                    double seconds;
                    double price;

                    (distance, end) = Extract(mail, ">\r\nTotal Distance (");
                    (minutes, end) = Extract(mail, ">\r\nTotal Time (", "m", end);
                    (seconds, end) = Extract(mail, " ", "s", end);
                    (price, _) = Extract(mail, "direction:ltr; font-weight:Bold\">\r\n", begin: end);

                    end = 0;

                    result = new ScooterRide
                    {
                        Price = price,
                        Distance = distance * 1000,
                        Time = minutes * 60 + seconds
                    };
                }
                catch (Exception e)
                {
                    result = default;
                    Console.WriteLine($"Cannot parse Circ mail: {e}");
                }

                yield return result;
            }
        }
    }
}