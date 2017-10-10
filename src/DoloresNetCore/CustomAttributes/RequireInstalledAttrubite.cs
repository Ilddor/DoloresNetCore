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
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var configs = map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(context.Guild.Id);

            if (guildConfig.InstalledModules.Contains(command.Module.Name))
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Module not installed, contact server owner");
        }
    }
}
