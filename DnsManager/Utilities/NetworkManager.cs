using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DnsManager
{
    public class NetworkManager
    {
       public static void SetDNS(params string[] Dns)
        {
            var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            objdns["DNSServerSearchOrder"] = Dns;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }
        public static void UnsetDNS()
        {
            var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            objdns["DNSServerSearchOrder"] = null;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }
        public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
        {
            var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

            return Nic;
        }

        /// <summary>
        /// Get current system DNS servers from the active network interface
        /// </summary>
        public static List<string> GetCurrentSystemDNS()
        {
            try
            {
                var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
                if (CurrentInterface == null) return new List<string>();

                var dnsServers = CurrentInterface.GetIPProperties().DnsAddresses
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToList();

                return dnsServers;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Ping a remote host to test connectivity
        /// </summary>
        public static async Task<(bool Success, int ResponseTimeMs)> PingHostAsync(string hostNameOrAddress, int timeoutMs = 4000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(hostNameOrAddress, timeoutMs);
                    return (reply.Status == IPStatus.Success, (int)reply.RoundtripTime);
                }
            }
            catch
            {
                return (false, timeoutMs);
            }
        }

        /// <summary>
        /// Test DNS resolution for a specific hostname
        /// </summary>
        public static async Task<(bool Success, List<string> Addresses, string ErrorMessage)> ResolveDnsAsync(string hostName)
        {
            try
            {
                var ipHostEntry = await Dns.GetHostEntryAsync(hostName);
                var addresses = ipHostEntry.AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToList();

                return (addresses.Count > 0, addresses, null);
            }
            catch (Exception ex)
            {
                return (false, new List<string>(), ex.Message);
            }
        }

        /// <summary>
        /// Test DNS server by querying common hostnames
        /// </summary>
        public static async Task<(bool Success, List<string> Results)> TestDnsServerAsync(string dnsServerIp, string hostToResolve = "google.com")
        {
            try
            {
                var results = new List<string>();

                // Test basic DNS resolution
                var (success, addresses, error) = await ResolveDnsAsync(hostToResolve);
                if (success)
                {
                    results.Add($"✓ Resolved {hostToResolve}: {string.Join(", ", addresses)}");
                    return (true, results);
                }
                else
                {
                    results.Add($"✗ Failed to resolve {hostToResolve}: {error}");
                    return (false, results);
                }
            }
            catch (Exception ex)
            {
                return (false, new List<string> { $"✗ Error testing DNS: {ex.Message}" });
            }
        }

        /// <summary>
        /// Flush DNS cache (requires admin privileges)
        /// </summary>
        public static bool FlushDnsCache()
        {
            try
            {
                var process = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(process))
                {
                    proc.WaitForExit();
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get detailed DNS settings from all network interfaces
        /// </summary>
        public static List<(string InterfaceName, List<string> DnsServers)> GetAllDnsSettings()
        {
            var result = new List<(string, List<string>)>();

            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        var dnsServers = nic.GetIPProperties().DnsAddresses
                            .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                            .Select(a => a.ToString())
                            .ToList();

                        if (dnsServers.Any())
                        {
                            result.Add((nic.Name, dnsServers));
                        }
                    }
                }
            }
            catch
            {
                // Silent failure
            }

            return result;
        }
    }
}
