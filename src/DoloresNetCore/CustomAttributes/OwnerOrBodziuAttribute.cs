using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Dolores.CustomAttributes
{
    class OwnerOrBodziuAttribute : RequireOwnerAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            if(context.User.Id == 132131643849834497)
            {
                return new Task<PreconditionResult>(PreconditionResult.FromSuccess);
            }
            else
            {
                return base.CheckPermissionsAsync(context, command, map);
            }
        }
    }
}
