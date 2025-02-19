namespace DiscordTemplate;

public static class ApiConfiguration
{
    public static string ClientId { get; set; } = Environment.GetEnvironmentVariable("CLIENT_ID");
    public static string ClientSecret { get; set; } = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    public static string BaseUrl { get; set; } = Environment.GetEnvironmentVariable("BASE_URL");

    public static string ResponseType { get; set; } = Environment.GetEnvironmentVariable("RESPONSE_TYPE");
    public static string State { get; set; } = Environment.GetEnvironmentVariable("STATE");
    public static string Scope { get; set; } = Environment.GetEnvironmentVariable("SCOPE");

    public static string AuthorizationEndpoint => BaseUrl + "authorize";
    public static string TokenEndpoint => BaseUrl + "token";
    public static string GetLicensesEndpoint => BaseUrl + "protected/get-licenses";

}

