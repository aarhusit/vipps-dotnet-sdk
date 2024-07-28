using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vipps.net.Exceptions;
using Vipps.net.Helpers;
using Vipps.net.Models.Recurring;

namespace Vipps.net.Infrastructure
{
    internal abstract class BaseServiceClient
    {
        protected readonly IVippsHttpClient _vippsHttpClient;
        protected readonly ILogger _logger;

        protected BaseServiceClient(IVippsHttpClient vippsHttpClient, ILoggerFactory loggerFactory)
        {
            _vippsHttpClient = vippsHttpClient;
            _logger = loggerFactory?.CreateLogger(this.GetType().Name);
        }

        public async Task<TResponse> ExecuteRequest<TRequest, TResponse>(
            string path,
            HttpMethod httpMethod,
            TRequest data,
            CancellationToken cancellationToken = default
        )
            where TRequest : class
            where TResponse : class
        {
            return await ExecuteRequestBaseAndParse<TResponse>(
                path,
                httpMethod,
                CreateRequestContent(data),
                null,
                cancellationToken
            );
        }

        public async Task ExecuteRequest<TRequest>            (string path, HttpMethod httpMethod, TRequest data, CancellationToken cancellationToken = default) where TRequest : class
        {
            await ExecuteRequestBase(
                path,
                httpMethod,
                CreateRequestContent(data),
                null,
                cancellationToken
            );
        }

        public async Task<TResponse> ExecuteRequest<TResponse>(string path, HttpMethod httpMethod, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await ExecuteRequestBaseAndParse<TResponse>(
                path,
                httpMethod,
                null,
                null,
                cancellationToken
            );
        }

        public async Task<TResponse> ExecuteRequestWithHeader<TResponse>(string path, HttpMethod httpMethod, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await ExecuteRequestBaseAndParse<TResponse>(
                path,
                httpMethod,
                null,
                headers,
                cancellationToken
            );
        }

        protected virtual async Task<Dictionary<string, string>> GetHeaders(
            CancellationToken cancellationToken
        )
        {
            return await Task.FromResult((Dictionary<string, string>)null);
        }

        private async Task<TResponse> ExecuteRequestBaseAndParse<TResponse>(
            string path,
            HttpMethod httpMethod,
            HttpContent httpContent,
            Dictionary<string, string> headers,
            CancellationToken cancellationToken
        )
            where TResponse : class
        {
            var response = await ExecuteRequestBase(
                path,
                httpMethod,
                httpContent,
                headers,
                cancellationToken
            );
#pragma warning disable IDE0079 // Remove unnecessary suppression. This is caused by us building multiple targets. In some (net 6, 7), the overload with the cancellationToken is preferred. In the others, it does not exist.
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
#pragma warning restore IDE0079 // Remove unnecessary suppression
            try
            {
                var responseObject = VippsRequestSerializer.DeserializeVippsResponse<TResponse>(
                    contentString
                );
                return responseObject is null
                    ? throw new VippsTechnicalException("Deserialization returned null")
                    : responseObject;
            }
            catch (Exception ex)
            {
                throw new VippsTechnicalException(
                    $"Error deserializing response of type {nameof(TResponse)}",
                    ex
                );
            }
        }

        private async Task<HttpResponseMessage> ExecuteRequestBase(
            string path,
            HttpMethod httpMethod,
            HttpContent httpContent,
            Dictionary<string, string> extraHeaders,
            CancellationToken cancellationToken
        )
        {
            var absolutePath = $"{_vippsHttpClient.BaseAddress}{path}";
            var retryPolicy = PolicyHelper.GetRetryPolicyWithFallback(
                _logger,
                $"Request to {httpMethod.Method} {absolutePath} failed even after retries"
            );
            var headers = await GetHeaders(cancellationToken);

            if (extraHeaders != null)
                foreach (var extraHeader in extraHeaders)
                    headers.Add(extraHeader.Key, extraHeader.Value);

            HttpResponseMessage response = null;
            try
            {
                response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var requestMessage = new HttpRequestMessage
                    {
                        RequestUri = new Uri(absolutePath),
                        Method = httpMethod,
                        Content = httpContent,
                    };
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            AddOrUpdateHeader(requestMessage.Headers, item.Key, item.Value);
                        }
                    }

                    return await _vippsHttpClient
                        .SendAsync(requestMessage, cancellationToken)
                        .ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                throw new VippsTechnicalException(
                    $"Request to {httpMethod.Method} {absolutePath} failed with exception: '{ex.Message}'.",
                    ex
                );
            }

            if (!response.IsSuccessStatusCode)
            {
#pragma warning disable IDE0079 // Remove unnecessary suppression. This is caused by us building multiple targets. In some (net 6, 7), the overload with the cancellationToken is preferred. In the others, it does not exist.
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
                var responseContent = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
#pragma warning restore IDE0079 // Remove unnecessary suppression
                var errorMessage =
                    $"Request to {httpMethod.Method} {absolutePath} failed with status code {response.StatusCode}, content: '{responseContent}'";
                var statusCode = (int)response.StatusCode;
                const int statusCodeBadRequest = (int)System.Net.HttpStatusCode.BadRequest;
                const int statusCodeInternalServerError = (int)System.Net.HttpStatusCode.InternalServerError;

                //If StatusCode is 400...499
                
                if (statusCode is >= statusCodeBadRequest and < statusCodeInternalServerError)
                {
                    //Catch Recurring error
                    if (statusCode == statusCodeBadRequest)
                    {
                        var error = VippsRequestSerializer.DeserializeVippsResponse<ErrorV3>(responseContent);
                        if (error != null)
                        {
                            throw new VippsRecurringException(errorMessage, null, error);
                        }
                    }
                    
                    throw new VippsUserException(errorMessage);
                }

                throw new VippsTechnicalException(errorMessage);
            }

            return response;
        }

        private static StringContent CreateRequestContent<TRequest>(TRequest vippsRequest)
            where TRequest : class
        {
            if (vippsRequest is null)
            {
                return null;
            }
            var serializedRequest = VippsRequestSerializer.SerializeVippsRequest(vippsRequest);
            return new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        }

        private static void AddOrUpdateHeader(HttpHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
            {
                headers.Remove(key);
            }

            headers.Add(key, value);
        }
    }
}
