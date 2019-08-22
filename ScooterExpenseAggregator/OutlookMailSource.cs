using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using NLog;

namespace Realmar.ScooterExpenseAggregator
{
    public class OutlookMailSource : IMailDataSource, IAsyncInitializable
    {
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        private IGraphServiceClient _client;

        public async Task InitializeAsync()
        {
            // User.Read --> Needed for sign in
            // Mail.Read --> Read actual mails
            var appScopes = new[] {"User.Read", "Mail.Read"};
            var appId = "bec1f446-9c59-4b92-9647-a3e872bf2bb2";

            var clientApplication = PublicClientApplicationBuilder
                .Create(appId)
                .WithRedirectUri("http://localhost:1234")
                .Build();

            var authenticationResult = await clientApplication
                .AcquireTokenInteractive(appScopes)
                .ExecuteAsync()
                .ConfigureAwait(false);

            var authenticationProvider = new DelegateAuthenticationProvider(message =>
            {
                message.Headers.Authorization =
                    new AuthenticationHeaderValue("bearer", authenticationResult.AccessToken);
                return Task.FromResult(0);
            });

            _client = new GraphServiceClient(authenticationProvider);
        }

        public async IAsyncEnumerable<string> EnumerateMailsAsync(string address)
        {
            var messages = await _client.Me.Messages
                .Request()
                .Filter($"from/emailAddress/address eq '{address}'")
                .GetAsync()
                .ConfigureAwait(false);
            IUserMessagesCollectionRequest nextPageRequest = null;

            do
            {
                nextPageRequest = messages.NextPageRequest;
                foreach (var message in messages)
                {
                    _logger.Debug(
                        $"Reading message from {message.ReceivedDateTime:R} {message.From.EmailAddress.Address} {message.Id}");
                    yield return message.Body.Content;
                }

                if (nextPageRequest != null)
                {
                    messages = await nextPageRequest.GetAsync().ConfigureAwait(false);
                }
            } while (nextPageRequest != null);
        }
    }
}