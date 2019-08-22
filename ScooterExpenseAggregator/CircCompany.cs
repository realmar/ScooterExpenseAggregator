using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using static Realmar.ScooterExpenseAggregator.DataHelper;

namespace Realmar.ScooterExpenseAggregator
{
    public class CircCompany : IScooterCompany
    {
        private readonly Regex _priceRegex = new Regex(@"Bold"">\s+(?<Price>\d+\.\d+)\s", RegexOptions.Compiled);

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
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

                    (distance, end) = Extract(mail, "Total Distance (");
                    (minutes, end) = Extract(mail, "Total Time (", "m", end);
                    (seconds, end) = Extract(mail, " ", "s", end);
                    var price = ParseOrDefault(_priceRegex.Match(mail).Groups["Price"].Value);

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
                    _logger.Warn(e, "Cannot parse Circ mail");
                    continue;
                }

                yield return result;
            }
        }
    }
}