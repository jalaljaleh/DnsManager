using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsManager
{ 
    /// <summary>
    /// Handles DNS protocol configuration and validation
    /// Manages DNS-over-TLS (DoT), DNS-over-HTTPS (DoH), and DNS-over-QUIC (DoQ)
    /// </summary>
    public class DnsProtocolHandler
    {
        /// <summary>
        /// Protocol configuration details
        /// </summary>
        public class ProtocolConfig
        {
            public DnsProtocolType Protocol { get; set; }
            public int DefaultPort { get; set; }
            public string Description { get; set; }
            public string Example { get; set; }
            public bool RequiresHostname { get; set; }
            public string ValidationRules { get; set; }
        }

        /// <summary>
        /// Get all available protocol configurations
        /// </summary>
        public static Dictionary<DnsProtocolType, ProtocolConfig> GetProtocolConfigs()
        {
            return new Dictionary<DnsProtocolType, ProtocolConfig>
            {
                {
                    DnsProtocolType.Standard, new ProtocolConfig
                    {
                        Protocol = DnsProtocolType.Standard,
                        DefaultPort = 53,
                        Description = "Standard DNS (UDP)",
                        Example = "8.8.8.8 / 8.8.4.4",
                        RequiresHostname = false,
                        ValidationRules = "Valid IP address required"
                    }
                },
                {
                    DnsProtocolType.TLS, new ProtocolConfig
                    {
                        Protocol = DnsProtocolType.TLS,
                        DefaultPort = 853,
                        Description = "DNS over TLS (DoT) - Encrypted",
                        Example = "8.8.8.8:853 | Hostname: dns.google",
                        RequiresHostname = true,
                        ValidationRules = "Valid IP + Hostname required for certificate verification"
                    }
                },
                {
                    DnsProtocolType.HTTPS, new ProtocolConfig
                    {
                        Protocol = DnsProtocolType.HTTPS,
                        DefaultPort = 443,
                        Description = "DNS over HTTPS (DoH) - Encrypted via HTTP/2",
                        Example = "94.140.14.14:443 | Hostname: dns.adguard.com | Path: /dns-query",
                        RequiresHostname = true,
                        ValidationRules = "Valid IP + Hostname + Path required"
                    }
                },
                {
                    DnsProtocolType.QUIC, new ProtocolConfig
                    {
                        Protocol = DnsProtocolType.QUIC,
                        DefaultPort = 443,
                        Description = "DNS over QUIC (DoQ) - Encrypted via QUIC",
                        Example = "8.8.8.8:443 | Hostname: dns.google | (Experimental)",
                        RequiresHostname = true,
                        ValidationRules = "Valid IP + Hostname required"
                    }
                }
            };
        }

        /// <summary>
        /// Get configuration for a specific protocol
        /// </summary>
        public static ProtocolConfig GetProtocolConfig(DnsProtocolType protocol)
        {
            var configs = GetProtocolConfigs();
            return configs.TryGetValue(protocol, out var config) ? config : null;
        }

        /// <summary>
        /// Validate DNS item configuration for the selected protocol
        /// </summary>
        public static (bool IsValid, List<string> Errors) ValidateDnsItem(DnsItem item)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add("DNS name is required");

            if (string.IsNullOrWhiteSpace(item.DnsAddress))
                errors.Add("DNS address is required");

            // Validate IP address format
            if (!string.IsNullOrWhiteSpace(item.DnsAddress) && !IsValidIpAddress(item.DnsAddress))
                errors.Add("Invalid DNS address format (must be valid IP address)");

            // Protocol-specific validation
            var config = GetProtocolConfig(item.Protocol);
            if (config != null && config.RequiresHostname)
            {
                if (string.IsNullOrWhiteSpace(item.HostName))
                    errors.Add($"{item.Protocol} protocol requires a hostname (e.g., dns.google, dns.adguard.com)");

                // Validate hostname format
                if (!string.IsNullOrWhiteSpace(item.HostName) && !IsValidHostname(item.HostName))
                    errors.Add("Invalid hostname format");
            }

            // Validate custom port if provided
            if (item.CustomPort.HasValue && (item.CustomPort.Value < 1 || item.CustomPort.Value > 65535))
                errors.Add("Custom port must be between 1 and 65535");

            // Validate HTTPS path
            if (item.Protocol == DnsProtocolType.HTTPS && string.IsNullOrWhiteSpace(item.HttpsPath))
                errors.Add("HTTPS path is required for DoH (e.g., /dns-query)");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Get the configuration string for connecting to a DNS server
        /// </summary>
        public static string GetConnectionString(DnsItem item)
        {
            var port = item.GetEffectivePort();

            switch (item.Protocol)
            {
                case DnsProtocolType.Standard:
                    return $"{item.DnsAddress}:{port}";

                case DnsProtocolType.TLS:
                    return $"tls://{item.DnsAddress}:{port} (SNI: {item.HostName})";

                case DnsProtocolType.HTTPS:
                    return $"https://{item.HostName}:{port}{item.HttpsPath}";

                case DnsProtocolType.QUIC:
                    return $"quic://{item.DnsAddress}:{port} (SNI: {item.HostName})";

                default:
                    return $"{item.DnsAddress}:{port}";
            }
        }


        /// <summary>
        /// Get protocol-specific connection details for documentation
        /// </summary>
        public static string GetProtocolDetails(DnsItem item)
        {
            var details = new StringBuilder();
            details.AppendLine($"Protocol: {item.Protocol}");
            details.AppendLine($"DNS Address: {item.DnsAddress}");

            if (!string.IsNullOrWhiteSpace(item.DnsAddressAlt))
                details.AppendLine($"Alternative DNS: {item.DnsAddressAlt}");

            details.AppendLine($"Port: {item.GetEffectivePort()}");

            if (!string.IsNullOrWhiteSpace(item.HostName))
                details.AppendLine($"Hostname: {item.HostName}");

            if (item.Protocol == DnsProtocolType.HTTPS)
                details.AppendLine($"HTTPS Path: {item.HttpsPath}");

            if (item.IsValidated)
            {
                details.AppendLine($"Status: ✓ Validated");
                if (item.LastTestedAt.HasValue)
                    details.AppendLine($"Last Tested: {item.LastTestedAt:G}");
                if (item.ResponseTimeMs.HasValue)
                    details.AppendLine($"Response Time: {item.ResponseTimeMs}ms");
            }
            else
            {
                details.AppendLine("Status: ⚠️ Not yet validated");
            }

            if (!string.IsNullOrWhiteSpace(item.Notes))
                details.AppendLine($"Notes: {item.Notes}");

            return details.ToString();
        }

        /// <summary>
        /// Validate IP address format
        /// </summary>
        private static bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// Validate hostname format (simplified check)
        /// </summary>
        private static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname) || hostname.Length > 253)
                return false;

            // Check for valid hostname characters (alphanumeric, dots, hyphens)
            return System.Text.RegularExpressions.Regex.IsMatch(
                hostname,
                @"^(?!-)([a-zA-Z0-9-]{1,63}(?<!-)\.)*[a-zA-Z0-9-]{1,63}(?<!-)$"
            );
        }

        /// <summary>
        /// Get human-readable protocol description
        /// </summary>
        public static string GetProtocolDescription(DnsProtocolType protocol)
        {
            var config = GetProtocolConfig(protocol);
            return config?.Description ?? protocol.ToString();
        }

        /// <summary>
        /// Suggest appropriate protocol based on use case
        /// </summary>
        public static DnsProtocolType SuggestProtocol(bool needsPrivacy, bool needsPerformance)
        {
            if (needsPrivacy)
                return DnsProtocolType.TLS; // Good balance of privacy and compatibility

            return DnsProtocolType.Standard; // Default for performance
        }
    }
}
