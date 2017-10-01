using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.EventHandlers
{
    interface IInstallable
    {
        Task Install(IServiceProvider map);
    }
}
