using Discord.Interactions;
using Discord.WebSocket;
using DiscordTemplate.AuthClient;
using DiscordTemplate.Services.Licenses;

namespace DiscordTemplate.Modules;


public class LicenseModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IOAuthClient _authClient;
    private readonly ILicenseAuthService _licenseService;

    public LicenseModule(IOAuthClient authClient, ILicenseAuthService licenseService)
    {
        _authClient = authClient;
        _licenseService = licenseService;
    }

    [SlashCommand("redeem-code", "redeem code given by the launcher")]
    public async Task HandleRedeemDiscordCode(string code)
    {
        // Acknowledge the command so Discord doesn't time out while processing
        await DeferAsync(ephemeral: true);

        var user = Context.User as SocketGuildUser;

        if(user == null)
        {
            await FollowupAsync("Something went wrong.");
            return;
        }

        // Get the access token
        var tokenResponse = await _authClient.GetAccessToken();
        if(tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            await FollowupAsync("Failed to retrieve access token.");
            return;
        }

        var isValid = Guid.TryParse(code, out var redeemCode);
        if(!isValid)
        {
            await FollowupAsync("Please input a valid license.");
            return;
        }

        await FollowupAsync("License Redeemed Successfully!");
    }

}
