using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharp.Models
{
    public class RemoteNode
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IEnumerable<int> RelatedItemIds { get; set; }
    }
}
