using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharp.Models
{
    public interface IBiggerThing
    {
        IThing Thing { get; set; }
        int OtherValue { get; set; }
    }
}
