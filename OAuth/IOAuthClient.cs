namespace DiscordTemplate.AuthClient
{
    public interface IOAuthClient
    {
        Task<TokenResponse?> GetAccessToken();
    }
}
