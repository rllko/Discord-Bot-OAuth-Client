using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DiscordTemplate.AuthClient
{
    internal class OAuthClient : IOAuthClient
    {
        private readonly HttpClient _httpClient;

        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
            Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(x => x.StatusCode is >= HttpStatusCode.InternalServerError
                or HttpStatusCode.RequestTimeout )
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2),5));

        public OAuthClient(HttpClient httpClient, IConfigurationRoot configuration)
        {
            _httpClient = httpClient;
        }


        private async Task<AuthorizationResponse?> GetCode()
        {
            var coveVerifier = generateCodeChallenge();

            var query =
            $"?response_type={ApiConfiguration.ResponseType}" +
            $"&client_id={ApiConfiguration.ClientId}" +
            $"&code_challenge=" + coveVerifier +
            $"&scope={ApiConfiguration.Scope}" +
            $"&state={ApiConfiguration.State}";

            // Send Request
            var httpResponseMessage = await _retryPolicy.ExecuteAsync(  () =>
            {
                // Create Request and add headers
                var requestMessage = new HttpRequestMessage(
                    HttpMethod.Get, ApiConfiguration.AuthorizationEndpoint + query)
                {
                    Headers =
                    {
                        { "Accept", "application/json" }
                    }
                };

                var result = _httpClient.SendAsync(requestMessage);
                return result;
            });

            // Get the response as JSON and Deserialize it
            var responseAsJson = await httpResponseMessage.Content.ReadAsStringAsync();

            if(httpResponseMessage.IsSuccessStatusCode is false)
            {
                Console.WriteLine(httpResponseMessage.StatusCode);
                return null;
            }

            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(responseAsJson);

            // Rerturn the Access Token
            return authorizationResponse;
        }

        public async Task<TokenResponse?> GetAccessToken()
        {
            var accessCode = await GetCode();

            // Send Request
            var httpResponseMessage = await _retryPolicy.ExecuteAsync( () =>
            {
                // Create Request and add headers
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiConfiguration.TokenEndpoint)
                {
                    Content =
                    new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "client_id", ApiConfiguration.ClientId },
                        { "client_secret", ApiConfiguration.ClientSecret },
                        { "code", accessCode.code },
                    }),
                    Headers =
                    {
                        { "Accept", "application/json"}
                    }
                };

                return _httpClient.SendAsync(requestMessage);
            });

            var responseAsJson = await httpResponseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(responseAsJson);
            var TokenRequest = JsonConvert.DeserializeObject<TokenResponse>(responseAsJson);
            return TokenRequest;
        }


        #region Helper Methods
        private string generateCodeChallenge()
        {
            var rng = RandomNumberGenerator.Create();

            var bytes = new byte[32];
            rng.GetBytes(bytes);

            // It is recommended to use a URL-safe string as code_verifier.
            // See section 4 of RFC 7636 for more details.
            var code_verifier = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

            using var sha256 = SHA256.Create();

            var challengeBytes = sha256.ComputeHash(
                 Encoding.UTF8.GetBytes(code_verifier));

            return Convert.ToBase64String(challengeBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        #endregion
    }

    #region Helper Classes

    public sealed class AuthorizationResponse
    {
        public string code { get; set; }
        public double expiresIn { get; set; }
        public List<string> scope { get; set; }
        public string tokenType { get; set; }
    }

    public sealed class TokenResponse
    {
        public string AccessToken { get; set; }
        public string? IdentityToken { get; set; }
        public string TokenType { get; set; }
        public DateTime ExpiresIn { get; set; }
        public string Scope { get; set; }
    }

    #endregion  
}