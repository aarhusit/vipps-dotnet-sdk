using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vipps.net.Helpers;

namespace Vipps.net.Infrastructure
{
    public class VippsHttpClient(HttpClient httpClient, VippsConfigurationOptions options) : IVippsHttpClient
    {
        private readonly TimeSpan _defaultTimeOut = TimeSpan.FromSeconds(100);

        public Uri BaseAddress
        {
            get { return HttpClient.BaseAddress; }
        }

        internal HttpClient HttpClient => httpClient ??= CreateDefaultHttpClient();

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = GetHeaders();
            foreach (var header in headers)
            {
                if (request.Headers.Contains(header.Key))
                {
                    request.Headers.Remove(header.Key);
                }

                request.Headers.Add(header.Key, header.Value);
            }

            var response = await HttpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
            return response;
        }

        private HttpClient CreateDefaultHttpClient()
        {
            var httpClient = new HttpClient
            {
                Timeout = _defaultTimeOut,
                BaseAddress = new Uri(UrlHelper.GetBaseUrl(options.UseTestMode))
            };

            return httpClient;
        }

        private Dictionary<string, string> GetHeaders()
        {
            var assemblyName = typeof(VippsApi).Assembly.GetName();
            var assemblyVersion = assemblyName.Version?.ToString();
            return new Dictionary<string, string>
            {
                { "User-Agent", $"Vipps/DotNet SDK/{assemblyVersion}" },
                { "Vipps-System-Name", assemblyName.Name },
                { "Vipps-System-Version", assemblyVersion },
                { "Merchant-Serial-Number", options.MerchantSerialNumber },
                { "Vipps-System-Plugin-Name", string.IsNullOrWhiteSpace(options.PluginName) ? "acme-plugin" : options.PluginName },
                { "Vipps-System-Plugin-Version", string.IsNullOrWhiteSpace(options.PluginVersion) ? "0.0.1" : options.PluginVersion }
            };
        }
    }
}
