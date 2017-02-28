using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharp.Models
{
    public class Node
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IEnumerable<Node> Children { get; set; }
    }
}
