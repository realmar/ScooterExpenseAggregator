using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ScooterExpenseAggregator.Properties;
using static Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest;

namespace Realmar.ScooterExpenseAggregator
{
    public class GmailMailSource : IMailDataSource
    {
        private GmailService _client;

        public async Task InitializeAsync()
        {
            var applicationName = "Scooter Expense Aggregator";
            var credentials = Resources.GoogleCredentials;
            await using var credentialsStream = new MemoryStream(Encoding.UTF8.GetBytes(credentials));
            var scopes = new[] {GmailService.Scope.GmailReadonly};

            var credPath = "token.json";
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(credentialsStream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true))
                .ConfigureAwait(false);

            _client = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
        }

        public async IAsyncEnumerable<string> EnumerateMailsAsync(string address)
        {
            string nextPageToken = null;
            do
            {
                var request = _client.Users.Messages.List("me");
                request.IncludeSpamTrash = true;
                request.PageToken = nextPageToken;
                request.Q = $"from:{address}";

                var result = await request.ExecuteAsync().ConfigureAwait(false);
                nextPageToken = result.NextPageToken;

                if (result.Messages == null)
                {
                    yield break;
                }
                else
                {
                    foreach (var message in result.Messages)
                    {
                        var getRequest = _client.Users.Messages
                            .Get("me", message.Id);
                        getRequest.Format = FormatEnum.Full;
                        var actualMessage = await getRequest
                            .ExecuteAsync()
                            .ConfigureAwait(false);

                        yield return GetBody(actualMessage);
                    }
                }
            } while (nextPageToken != null);
        }

        private static string GetBody(Message message)
        {
            var sb = new StringBuilder();

            foreach (var part in message.Payload.Parts)
            {
                sb.Append(DecodePart(part));
            }

            return sb.ToString();
        }

        private static string DecodePart(MessagePart part)
        {
            var input = part.Body.Data;
            if (string.IsNullOrWhiteSpace(input))
            {
                return "<strong>Message body was not returned from Google</strong>";
            }

            var InputStr = input.Replace("-", "+").Replace("_", "/");
            return Encoding.UTF8.GetString(Convert.FromBase64String(InputStr));
        }
    }
}