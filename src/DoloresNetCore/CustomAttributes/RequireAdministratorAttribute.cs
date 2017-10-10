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
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    class RequireAdministratorAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            if ((context.User as SocketGuildUser).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("You need to have admin privilages to use this command");
        }
    }
}
