using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace Realmar.ScooterExpenseAggregator
{
    internal static class MailSourceFactory
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        internal static async Task<IMailDataSource> Create(string sourceName)
        {
            var sourceType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IMailDataSource).IsAssignableFrom(type))
                .FirstOrDefault(type => type.Name.StartsWith(sourceName, StringComparison.InvariantCultureIgnoreCase));

            if (sourceType == null)
            {
                throw new ArgumentException($"Cannot find mail data provider with name {sourceName}", sourceName);
            }

            _logger.Info($"Using mail data source {sourceName}");
            var source = Activator.CreateInstance(sourceType);
            if (source is IAsyncInitializable initializable)
            {
                _logger.Debug("Initializing mail data source");
                await initializable.InitializeAsync().ConfigureAwait(false);
                _logger.Debug("Mail data source initialized");
            }

            return (IMailDataSource) source;
        }
    }
}