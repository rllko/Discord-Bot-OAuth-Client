using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DiscordTemplate.Services.Licenses
{
    internal class LicenseAuthService : ILicenseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration? _api;

        public LicenseAuthService(HttpClient httpClient, IConfigurationRoot configurationRoot)
        {
            _api = configurationRoot.Get<ApiConfiguration>();
            _httpClient = httpClient;

            if(_api == null)
            {
                throw new Exception("Api is missing in the configuration file.");
            }
        }
        public async Task<List<string>> GetUserLicenses(string accessToken, ulong id)
        {
            var endpoint = $"{_api.getLicensesEndpoint }?discordId={id}";

            var request = new HttpRequestMessage(HttpMethod.Get,endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonContent =  JsonConvert.DeserializeObject<LicenseResponse>(content);

                return ((dynamic) jsonContent).result;
            }

            // Debug, please improve this later
            Console.WriteLine(response.StatusCode.ToString());
            return null;
        }
    }

    class LicenseResponse
    {
        public string error { get; set; }
        public List<string> result { get; set; }
    }

}
