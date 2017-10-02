using System;
using System.Collections.Generic;
using System.Text;

namespace Dolores.DataClasses
{
    interface IState
    {
        void Save();
        void Load();
    }
}
