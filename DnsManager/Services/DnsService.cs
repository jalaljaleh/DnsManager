using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DnsManager
{
    public class DnsService
    {
        public const string ItemsPath = "\\data.json";

        public List<DnsItem> DnsItems;
        public DnsService()
        {
            DnsItems = LoadDnsItems();

        }

        public DnsItem Connected { get => DnsItems.FirstOrDefault(a => a.IsConnected); }
        public bool ChangeConnection(DnsItem newDns)
        {
            if (Connected != null)
                Connected.IsConnected = false;

            newDns.IsConnected = true;
            return true;
        }
        private List<DnsItem> LoadDnsItems()
        {
            try
            {
                string data = File.ReadAllText(App.DirectoryPath + ItemsPath);
                var items = JsonConvert.DeserializeObject<List<DnsItem>>(data);
                return items;
            }
            catch
            {
                return new List<DnsItem>()
                {
                  //  new DnsItem(){Name="System Default",Description="The ISP provides the default DNS servers. These are automatically configured when you connect to the internet through their network. !",IsConnected = true,Priority=-1}
                };
            }
        }
        public bool SaveDnsItems()
        {
            try
            {
                string data = JsonConvert.SerializeObject(this.DnsItems);
                File.WriteAllText(App.DirectoryPath + ItemsPath, data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
