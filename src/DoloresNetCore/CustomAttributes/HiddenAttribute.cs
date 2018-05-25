using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;

namespace Dolores.CustomAttributes
{
    [AttributeUsage(
        AttributeTargets.Method |
        AttributeTargets.Class)]
    public class HiddenAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            return PreconditionResult.FromSuccess();
        }
    }
}
