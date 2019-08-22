using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using static Realmar.ScooterExpenseAggregator.DataHelper;

namespace Realmar.ScooterExpenseAggregator
{
    public class TierCompany : IScooterCompany
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMailDataSource _mailDataSource;

        public TierCompany(IMailDataSource mailDataSource)
        {
            _mailDataSource = mailDataSource;
        }

        public string Name => "Tier";

        public async IAsyncEnumerable<ScooterRide> EnumerateRidesAsync()
        {
            var mails = _mailDataSource.EnumerateMailsAsync("support@tier.app").ConfigureAwait(false);
            await foreach (var mail in mails)
            {
                var price = 0d;

                try
                {
                    (price, _) = Extract(mail, "Fr.", endIdentifier: "</p>");
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Cannot parse Tier mail");
                    continue;
                }

                yield return new ScooterRide
                {
                    Price = price
                };
            }
        }
    }
}