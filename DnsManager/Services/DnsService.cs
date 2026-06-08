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
        public DnsTestingService TestingService { get; private set; }

        public DnsService()
        {
            DnsItems = LoadDnsItems();
            TestingService = new DnsTestingService();

            // Ensure backward compatibility - migrate old items
            EnsureBackwardCompatibility();
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
                return items ?? new List<DnsItem>();
            }
            catch
            {
                return new List<DnsItem>();
            }
        }

        /// <summary>
        /// Ensure backward compatibility with older DNS items that don't have new properties
        /// </summary>
        private void EnsureBackwardCompatibility()
        {
            foreach (var item in DnsItems)
            {
                // Set defaults for new properties if not already set
                if (string.IsNullOrWhiteSpace(item.HostName))
                    item.HostName = string.Empty;

                if (item.Protocol == default(DnsProtocolType))
                    item.Protocol = DnsProtocolType.Standard;

                if (string.IsNullOrWhiteSpace(item.HttpsPath))
                    item.HttpsPath = "/dns-query";

                if (string.IsNullOrWhiteSpace(item.Notes))
                    item.Notes = string.Empty;
            }
        }

        public bool SaveDnsItems()
        {
            try
            {
                string data = JsonConvert.SerializeObject(this.DnsItems, Formatting.Indented);
                File.WriteAllText(App.DirectoryPath + ItemsPath, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all DNS items sorted by priority
        /// </summary>
        public List<DnsItem> GetAllDnsSorted()
        {
            return DnsItems.OrderBy(a => a.Priority).ToList();
        }

        /// <summary>
        /// Validate DNS item configuration
        /// </summary>
        public (bool IsValid, List<string> Errors) ValidateDnsItem(DnsItem item)
        {
            return DnsProtocolHandler.ValidateDnsItem(item);
        }

        /// <summary>
        /// Test a DNS server
        /// </summary>
        public async Task<DnsTestingService.DnsTestResult> TestDnsServerAsync(DnsItem item)
        {
            return await TestingService.TestDnsServerAsync(item);
        }

        /// <summary>
        /// Get all DNS servers including current system DNS
        /// </summary>
        public List<DnsItem> GetAllServersWithSystem()
        {
            var all = new List<DnsItem>(DnsItems);

            // Add system default DNS if not already present
            var systemDns = NetworkManager.GetCurrentSystemDNS();
            if (systemDns.Any() && !DnsItems.Any(a => a.Name == "System Default"))
            {
                all.Insert(0, new DnsItem
                {
                    Name = "System Default",
                    Description = "Your ISP's default DNS servers",
                    DnsAddress = systemDns.FirstOrDefault() ?? "Auto",
                    DnsAddressAlt = systemDns.Count > 1 ? systemDns[1] : string.Empty,
                    Priority = -1,
                    IsConnected = Connected == null, // Select if nothing else is connected
                    Protocol = DnsProtocolType.Standard,
                    Notes = "Automatically detected from network interface"
                });
            }

            return all;
        }
    }
}

