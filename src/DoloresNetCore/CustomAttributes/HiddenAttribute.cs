using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;

namespace Dolores.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HiddenAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            return PreconditionResult.FromSuccess();
        }
    }
}
