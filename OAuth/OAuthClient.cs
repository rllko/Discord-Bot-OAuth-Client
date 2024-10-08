﻿using Microsoft.Extensions.Configuration;
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
        private readonly ApiConfiguration? _api;
        private static TokenResponse? lastToken = null;

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

            _api = configuration.GetSection("ApiConfiguration").Get<ApiConfiguration>();

            if(_api == null)
            {
                throw new Exception("Api Configuration is missing in the configuration file.");
            }
        }


        private async Task<AuthorizationResponse?> GetCode()
        {
            var code_verifier = generateCodeChallenge();

            var query =
            $"?response_type={_api.response_type}" +
            $"&client_id={_api.clientId}" +
            $"&code_challenge=" + code_verifier +
            $"&code_challenge_method={_api.code_challenge_method}" +
            $"&scope={_api.scope}" +
            $"&state={_api.state}";

            // Send Request
            var httpResponseMessage = await _retryPolicy.ExecuteAsync(  () =>
            {
                // Create Request and add headers
                var requestMessage = new HttpRequestMessage(
                    HttpMethod.Get, _api.authorizationEndpoint + query)
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
            if(lastToken?.ExpiresIn < DateTime.Now)
            {
                lastToken.ExpiresIn = DateTime.UtcNow.AddMinutes(30);
                return lastToken;
            }

            var accessCode = await GetCode();

            // Send Request
            var httpResponseMessage = await _retryPolicy.ExecuteAsync( () =>
            {
                // Create Request and add headers
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _api.tokenEndpoint)
                {
                    Content =
                    new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "client_id", _api.clientId },
                        { "client_secret", _api.clientSecret },
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
            lastToken = TokenRequest;
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
