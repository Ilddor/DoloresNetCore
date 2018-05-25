using Discord;
using Discord.Commands;
using Dolores.DataClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;

namespace Dolores.CustomAttributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    class RequireInstalledAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var configs = map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = null;
            if (context.Channel is IDMChannel)
            {
                // Try to find guild that user belongs to and we know it, in case of more than one guild - decide which one to get context for
                guildConfig = configs.GetGuildFromDMContext(map.GetService<DiscordSocketClient>(), context.User.Id);

                if(guildConfig == null)
                    return PreconditionResult.FromError("You do not belong to any guild known to me");
            }
            else
            {
                guildConfig = configs.GetGuildConfig(context.Guild.Id);
            }

            if (guildConfig.InstalledModules.Contains(command.Module.Name))
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Module not installed, contact server owner");
        }
    }
}
