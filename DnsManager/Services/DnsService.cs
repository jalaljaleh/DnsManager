using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsManager
{
    public class DnsService
    {
        public const string ItemsPath = "items.json";

        public DnsService()
        {
            DnsItems = new List<DnsItem>();

        }
        public List<DnsItem> DnsItems;
        public void InvokeEvent()
        {
            OnDnsListChanged?.Invoke(this, new DnsListChangedEventArgs()
            {
                UpdatedList = this.DnsItems
            });
        }
        public void AddDns(DnsItem value)
        {
            DnsItems.Add(value);
            InvokeEvent();
        }
        public void RemoveDns(DnsItem value)
        {
            DnsItems.Remove(value);
            InvokeEvent();
        }
        public event EventHandler<DnsListChangedEventArgs> OnDnsListChanged;
        public class DnsListChangedEventArgs : EventArgs
        {
            public List<DnsItem> UpdatedList { get; set; }
        }
        public List<DnsItem> LoadDnsItems()
        {
            try
            {
                string data = File.ReadAllText(App.DirectoryPath + ItemsPath);
                var items = JsonConvert.DeserializeObject<List<DnsItem>>(data);
                return items;
            }
            catch
            {
                return default(List<DnsItem>);
            }
        }
        public bool SaveDnsItems(List<DnsItem> items)
        {
            try
            {
                string data = JsonConvert.SerializeObject(items);
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
