using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Vipps.net.Infrastructure;
using Vipps.net.Services;

namespace Vipps.net
{
    public interface IVippsApi
    {
        IVippsEpaymentService EpaymentService { get; }
        IVippsCheckoutService CheckoutService { get; }
        IVippsRecurringService RecurringService { get; }
    }

    public class VippsApi : IVippsApi
    {
        private readonly VippsConfigurationOptions _vippsConfigurationOptions;
        private readonly VippsHttpClient _vippsHttpClient;
        private readonly VippsAccessTokenService _accessTokenService;
        private readonly ILoggerFactory _loggerFactory;

        public VippsApi(
            VippsConfigurationOptions configurationOptions,
            HttpClient httpClient = null,
            ILoggerFactory loggerFactory = null
        )
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _vippsConfigurationOptions = configurationOptions;
            _vippsHttpClient = new VippsHttpClient(httpClient, configurationOptions);

            _accessTokenService = new VippsAccessTokenService(
                configurationOptions,
                new AccessTokenServiceClient(
                    configurationOptions,
                    _vippsHttpClient,
                    _loggerFactory
                ),
                new AccessTokenCacheService()
            );

            EpaymentService = new VippsEpaymentService(
                new EpaymentServiceClient(
                    _vippsConfigurationOptions,
                    _vippsHttpClient,
                    _accessTokenService,
                    _loggerFactory
                )
            );

            CheckoutService = new VippsCheckoutService(
                new CheckoutServiceClient(
                    _vippsConfigurationOptions,
                    _vippsHttpClient,
                    _loggerFactory
                )
            );

            RecurringService = new VippsRecurringService(
                new RecurringServiceClient(
                    _vippsConfigurationOptions,
                    _vippsHttpClient,
                    _accessTokenService,
                    _loggerFactory
                )
            );
        }

        public IVippsEpaymentService EpaymentService { get; }

        public IVippsCheckoutService CheckoutService { get; }

        public IVippsRecurringService RecurringService { get; }
    }
}
