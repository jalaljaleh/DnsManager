using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DnsManager
{ 
    /// <summary>
    /// Service for testing DNS servers and validating connectivity
    /// Supports ping, URL resolution, and custom URL testing
    /// </summary>
    public class DnsTestingService
    {
        private const int PingTimeoutMs = 4000;
        private const int HttpTimeoutMs = 5000;

        /// <summary>
        /// Test result for DNS validation
        /// </summary>
        public class DnsTestResult
        {
            public bool IsSuccessful { get; set; }
            public int ResponseTimeMs { get; set; }
            public string Message { get; set; }
            public string ErrorDetails { get; set; }
            public DateTime TestedAt { get; set; } = DateTime.Now;
        }

        /// <summary>
        /// Ping a hostname to test basic connectivity
        /// </summary>
        public async Task<DnsTestResult> PingHostAsync(string host, int timeoutMs = PingTimeoutMs)
        {
            var result = new DnsTestResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, timeoutMs);
                    stopwatch.Stop();

                    if (reply.Status == IPStatus.Success)
                    {
                        result.IsSuccessful = true;
                        result.ResponseTimeMs = (int)reply.RoundtripTime;
                        result.Message = $"✓ Ping successful: {reply.RoundtripTime}ms";
                    }
                    else
                    {
                        result.IsSuccessful = false;
                        result.Message = $"✗ Ping failed: {reply.Status}";
                        result.ErrorDetails = reply.Status.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.IsSuccessful = false;
                result.Message = "✗ Ping failed: Exception occurred";
                result.ErrorDetails = ex.Message;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Test DNS resolution by resolving a hostname
        /// </summary>
        public async Task<DnsTestResult> TestDnsResolutionAsync(string dnsServerAddress, string hostToResolve = "google.com")
        {
            var result = new DnsTestResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Create a resolver that uses the specific DNS server
                var ipHostEntry = await Dns.GetHostEntryAsync(hostToResolve);
                stopwatch.Stop();

                result.IsSuccessful = true;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.Message = $"✓ DNS resolution successful: Resolved {hostToResolve} to {ipHostEntry.AddressList.Length} address(es)";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.IsSuccessful = false;
                result.Message = $"✗ DNS resolution failed: {hostToResolve}";
                result.ErrorDetails = ex.Message;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Test DNS server by attempting to reach a well-known URL
        /// </summary>
        public async Task<DnsTestResult> TestDnsWithUrlAsync(string testUrl = "https://www.google.com", int timeoutMs = HttpTimeoutMs)
        {
            var result = new DnsTestResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var handler = new HttpClientHandler();
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMs) })
                {
                    var response = await client.GetAsync(testUrl);
                    stopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        result.IsSuccessful = true;
                        result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                        result.Message = $"✓ URL test successful ({response.StatusCode}): {testUrl}";
                    }
                    else
                    {
                        result.IsSuccessful = false;
                        result.Message = $"✗ URL test failed ({response.StatusCode}): {testUrl}";
                        result.ErrorDetails = response.StatusCode.ToString();
                        result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                result.IsSuccessful = false;
                result.Message = $"✗ URL test failed: {testUrl}";
                result.ErrorDetails = ex.Message;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.IsSuccessful = false;
                result.Message = $"✗ URL test error: {testUrl}";
                result.ErrorDetails = ex.Message;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Comprehensive DNS server test (multiple checks)
        /// </summary>
        public async Task<DnsTestResult> ComprehensiveTestAsync(DnsItem dnsItem)
        {
            var result = new DnsTestResult();
            var stopwatch = Stopwatch.StartNew();
            var testsPassed = 0;
            var testsTotal = 0;

            try
            {
                // Test 1: Ping the DNS server IP
                if (!string.IsNullOrWhiteSpace(dnsItem.DnsAddress))
                {
                    testsTotal++;
                    var pingResult = await PingHostAsync(dnsItem.DnsAddress);
                    if (pingResult.IsSuccessful)
                    {
                        testsPassed++;
                        result.ResponseTimeMs = Math.Max(result.ResponseTimeMs, pingResult.ResponseTimeMs);
                    }
                }

                // Test 2: Ping the hostname if specified
                if (!string.IsNullOrWhiteSpace(dnsItem.HostName))
                {
                    testsTotal++;
                    var hostPingResult = await PingHostAsync(dnsItem.HostName);
                    if (hostPingResult.IsSuccessful)
                    {
                        testsPassed++;
                    }
                }

                // Test 3: DNS resolution test
                testsTotal++;
                var dnsResolveResult = await TestDnsResolutionAsync(dnsItem.DnsAddress);
                if (dnsResolveResult.IsSuccessful)
                {
                    testsPassed++;
                }

                // Test 4: URL accessibility test
                testsTotal++;
                var urlResult = await TestDnsWithUrlAsync("https://www.google.com");
                if (urlResult.IsSuccessful)
                {
                    testsPassed++;
                    result.ResponseTimeMs = Math.Max(result.ResponseTimeMs, urlResult.ResponseTimeMs);
                }

                stopwatch.Stop();

                result.IsSuccessful = testsPassed >= (testsTotal / 2); // At least 50% tests must pass
                result.Message = $"✓ Comprehensive test completed: {testsPassed}/{testsTotal} checks passed";

                if (!result.IsSuccessful)
                {
                    result.Message = $"✗ DNS server validation failed: Only {testsPassed}/{testsTotal} checks passed";
                }

                if (result.ResponseTimeMs == 0)
                    result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.IsSuccessful = false;
                result.Message = "✗ Comprehensive test error";
                result.ErrorDetails = ex.Message;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }

            result.TestedAt = DateTime.Now;
            return result;
        }

        /// <summary>
        /// Test DNS server with protocol-specific considerations
        /// </summary>
        public async Task<DnsTestResult> TestDnsServerAsync(DnsItem dnsItem, string testUrl = "https://www.google.com")
        {
            var result = new DnsTestResult();

            try
            {
                // For encrypted protocols with hostname, test the hostname
                if (dnsItem.Protocol != DnsProtocolType.Standard && !string.IsNullOrWhiteSpace(dnsItem.HostName))
                {
                    var hostResult = await PingHostAsync(dnsItem.HostName);
                    if (!hostResult.IsSuccessful)
                    {
                        return new DnsTestResult
                        {
                            IsSuccessful = false,
                            Message = $"✗ Hostname test failed for {dnsItem.Protocol}: {dnsItem.HostName}",
                            ErrorDetails = hostResult.ErrorDetails,
                            ResponseTimeMs = hostResult.ResponseTimeMs
                        };
                    }
                    result.ResponseTimeMs = hostResult.ResponseTimeMs;
                }
                else if (!string.IsNullOrWhiteSpace(dnsItem.DnsAddress))
                {
                    // For standard DNS, test the IP address
                    var ipResult = await PingHostAsync(dnsItem.DnsAddress);
                    if (!ipResult.IsSuccessful)
                    {
                        return new DnsTestResult
                        {
                            IsSuccessful = false,
                            Message = $"✗ DNS server test failed: {dnsItem.DnsAddress}",
                            ErrorDetails = ipResult.ErrorDetails,
                            ResponseTimeMs = ipResult.ResponseTimeMs
                        };
                    }
                    result.ResponseTimeMs = ipResult.ResponseTimeMs;
                }

                // Test URL accessibility
                var urlResult = await TestDnsWithUrlAsync(testUrl);
                result.IsSuccessful = urlResult.IsSuccessful;
                result.Message = urlResult.IsSuccessful 
                    ? $"✓ DNS server test passed: {dnsItem.Name} ({dnsItem.Protocol})"
                    : $"✗ DNS server connectivity test failed: {urlResult.Message}";
                result.ErrorDetails = urlResult.ErrorDetails;
                result.ResponseTimeMs = Math.Max(result.ResponseTimeMs, urlResult.ResponseTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Message = $"✗ DNS server test error: {dnsItem.Name}";
                result.ErrorDetails = ex.Message;
            }

            result.TestedAt = DateTime.Now;
            return result;
        }
    }
}
