using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsManager
{
    public class DnsItem
    {
        public string Name { get; set; }
        public string DnsAddress { get; set; }
        public string DnsAddressAlt { get; set; }

        public string GetDisplayName() => Name + " " + DnsAddress + " " + DnsAddressAlt;
    }
}
