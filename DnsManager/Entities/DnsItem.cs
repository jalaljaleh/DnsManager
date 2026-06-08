using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsManager
{
    /// <summary>
    /// DNS Protocol Types supported for DNS queries
    /// </summary>
    public enum DnsProtocolType
    {
        /// <summary>Standard UDP DNS (Port 53)</summary>
        Standard = 0,
        /// <summary>DNS over TLS (DoT, typically Port 853)</summary>
        TLS = 1,
        /// <summary>DNS over HTTPS (DoH, typically Port 443)</summary>
        HTTPS = 2,
        /// <summary>DNS over QUIC (DoQ, typically Port 443)</summary>
        QUIC = 3
    }

    public class DnsItem
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string DnsAddress { get; set; }
        public string DnsAddressAlt { get; set; }

        public bool IsConnected { get; set; } = false;
        public int Priority { get; set; } = 999;

        /// <summary>
        /// DNS Protocol Type (Standard, TLS, HTTPS, or QUIC)
        /// </summary>
        public DnsProtocolType Protocol { get; set; } = DnsProtocolType.Standard;

        /// <summary>
        /// Custom hostname for DNS-over-TLS/HTTPS/QUIC (e.g., "dns.adguard.com" for AdGuard Home)
        /// Required for encrypted protocols and custom DNS services that don't expose client IDs via IP alone
        /// </summary>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// Custom port for DNS queries (default: 53 for Standard, 853 for TLS, 443 for HTTPS/QUIC)
        /// </summary>
        public int? CustomPort { get; set; }

        /// <summary>
        /// URL endpoint for DNS-over-HTTPS (e.g., "/dns-query")
        /// </summary>
        public string HttpsPath { get; set; } = "/dns-query";

        /// <summary>
        /// Indicates if the DNS server has been validated/tested
        /// </summary>
        public bool IsValidated { get; set; } = false;

        /// <summary>
        /// Timestamp of last successful DNS query test
        /// </summary>
        public DateTime? LastTestedAt { get; set; }

        /// <summary>
        /// Average response time in milliseconds from last test
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// User notes for the DNS server configuration
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets the effective port based on protocol and custom port setting
        /// </summary>
        public int GetEffectivePort()
        {
            if (CustomPort.HasValue && CustomPort.Value > 0)
                return CustomPort.Value;

            switch (Protocol)
            {
                case DnsProtocolType.TLS: return 853;
                case DnsProtocolType.HTTPS:
                case DnsProtocolType.QUIC: return 443;
                default: return 53;
            }
        

        }

        /// <summary>
        /// Gets display string combining protocol and hostname if applicable
        /// </summary>
        public string GetDisplayString()
        {
            var display = $"{DnsAddress}";
            if (!string.IsNullOrWhiteSpace(DnsAddressAlt))
                display += $" / {DnsAddressAlt}";

            if (Protocol != DnsProtocolType.Standard)
            {
                display += $" ({Protocol}";
                if (!string.IsNullOrWhiteSpace(HostName))
                    display += $": {HostName}";
                display += ")";
            }

            return display;
        }
    }
}
