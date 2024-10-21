using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDT2025.Models
{
    public class RecentPublication
    {
        public string Profile { get; set; }
        public string Date { get; set; }
        public string Server { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Total { get; set; }
    }

    public class RecentPublicationsWrapper
    {
        public Pubs Pubs { get; set; }
    }

    public class Pubs
    {
        public List<RecentPublication> Pub { get; set; }
    }

}
