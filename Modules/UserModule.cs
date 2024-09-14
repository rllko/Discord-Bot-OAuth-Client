using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordTemplate.AuthClient;
using DiscordTemplate.Services.Licenses;

namespace DiscordTemplate.Modules
{
    [CommandContextType(InteractionContextType.PrivateChannel, InteractionContextType.Guild, InteractionContextType.BotDm)]
    [IntegrationType(ApplicationIntegrationType.UserInstall)]
    public class UserModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IOAuthClient _authClient;
        private readonly ILicenseAuthService _licenseService;


        public UserModule(IOAuthClient authClient, ILicenseAuthService licenseService)
        {
            _authClient = authClient;
            _licenseService = licenseService;
        }

        [SlashCommand("airfryer", "He doesnt own one, imagine!")]
        public async Task DoesntOwnAirFryer(IUser user) => await RespondAsync($"{user.Mention} clearly doesnt own an Air Fryer, laugh at him!");

        [SlashCommand("license-list", "Shows a list with your owned licenses")]
        public async Task DisplayUserLicenses()
        {
            await DeferAsync(ephemeral: true);

            var tokenResponse = await _authClient.GetAccessToken();
            if(tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                await FollowupAsync("Something wrong happened.");
                return;
            }

            var currentUser = Context.User;
            var licenses = await _licenseService.GetUserLicenses(tokenResponse.AccessToken,currentUser.Id);

            if(licenses == null)
            {
                await FollowupAsync("You dont own any license");
                return;
            }

            await FollowupAsync($"""
            ```
            ************************************
            * Licenses Owned by {currentUser.GlobalName}
            ************************************

            {String.Join("\n", licenses.Select(license => $"{license}\nAcquired at: Date Here"))}

            Keep track of your licenses!
            Thank you for trusting in our mission!

            ************************************
            ```
            """);
        }

        [SlashCommand("reset-hwid", "reset's license hwid")]
        public async Task ResetHwidHandler()
        {
            await DeferAsync(ephemeral: true);

            var tokenResponse = await _authClient.GetAccessToken();
            if(tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                await FollowupAsync("Something wrong happened.");
                return;
            }

            var currentUserId = Context.User.Id;
            var licenses = await _licenseService.GetUserLicenses(tokenResponse.AccessToken,currentUserId);

            if(licenses == null)
            {
                await FollowupAsync("You dont own any license");
                return;
            }

            if(!licenses.Any())
            {
                await FollowupAsync("Something went wrong");
                return;
            }

            if(licenses.Count == 1)
            {
                await FollowupAsync("HWID reset was a Success!");
                return;
            }

            var selectMenu = new SelectMenuBuilder()
                .WithPlaceholder("Select a License to reset")
                .WithCustomId("license_selection:"+tokenResponse.AccessToken);

            licenses.ForEach(license =>
            {
                selectMenu.AddOption(new SelectMenuOptionBuilder(license, license));
            });

            var component = new ComponentBuilder().WithSelectMenu(selectMenu);
            await FollowupAsync("Please select a License", components: component.Build(), ephemeral: true);
        }

        [ComponentInteraction("license_selection:*")]
        public async Task HandleRace(string accessToken, string [] selected)
        {
            await DeferAsync(ephemeral: false);
            var user = Context.Interaction.User as SocketGuildUser;

            // var resetResponseSuccess = await _licenseService.ResetHwidAsync(accessToken,user.Id,selected[0]);
            await FollowupAsync("HWID reset successfully.", ephemeral: true);
            return;

        }
    }
}
