namespace DiscordTemplate;

public class ApiConfiguration
{
    public string clientId { get; set; } = Environment.GetEnvironmentVariable("CLIENT_ID");
    public string clientSecret { get; set; } = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    public string baseUrl { get; set; } = Environment.GetEnvironmentVariable("BASE_URL");

    public string response_type { get; set; }
    public string state { get; set; }
    public string scope { get; set; }
    public string code_challenge_method { get; set; }
    public string redirect_uri { get; set; }

    public string authorizationEndpoint => baseUrl + "authorize";
    public string tokenEndpoint => baseUrl + "token";
    public string getLicensesEndpoint => baseUrl + "protected/get-licenses";

}

