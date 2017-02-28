using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharp.Models
{
    public class BiggerThing : IBiggerThing
    {
        public IThing Thing { get; set; }
        public int OtherValue { get; set; }
    }
}
