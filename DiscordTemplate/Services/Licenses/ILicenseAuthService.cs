namespace DiscordTemplate.Services.Licenses
{
    public interface ILicenseAuthService
    {
        Task<List<string>> GetUserLicenses(string accessToken, ulong id);
    }
}
