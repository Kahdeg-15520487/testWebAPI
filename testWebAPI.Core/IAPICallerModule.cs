using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testWebAPI.Core
{
    public interface IAPICallerModule
    {
        string Name { get; }
        string Uri { get; }
    }
}
