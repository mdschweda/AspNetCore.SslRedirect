using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using H = Microsoft.Net.Http.Headers;

namespace MS.AspNetCore.Ssl {

    /// <inheritdoc/>
    internal class SslRedirector : ISslRedirector {

        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslRedirector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public SslRedirector(ILogger<SslRedirector> logger) => _logger = logger;

        /// <inheritdoc/>
        public async Task<bool> Accept(SslRedirectContext context, bool enforcePolicies = true) {
            var httpContext = context.HttpContext;
            var terminate = context.Options.Filter?.Invoke(context) ?? false;

            if (!terminate && !httpContext.Request.IsHttps && (!enforcePolicies || await EnforcePolicies(context))) {
                httpContext.Request.Scheme = "https";
                var host = new HostString(httpContext.Request.Host.Host, context.Options.SslPort);
                var builder = new UriBuilder("https", httpContext.Request.Host.Host, context.Options.SslPort) {
                    Path = httpContext.Request.PathBase + httpContext.Request.Path
                };
                if (httpContext.Request.QueryString.HasValue)
                    builder.Query = httpContext.Request.QueryString.Value;

                httpContext.Response.Headers[H.HeaderNames.Location] = builder.Uri.AbsoluteUri;

                var method = context.Options.Method;
                if (HttpMethods.IsGet(httpContext.Request.Method))
                    method = Fallback(method);
                httpContext.Response.StatusCode = (int)method;

                _logger.LogInformation("Redirecting request {path} to {host} using {code} - {method}",
                    httpContext.Request.Path, host, (int)method, method);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void AddHstsHeader(SslRedirectContext context) {
            if (context.HttpContext.Request.IsHttps && context.Options.HstsHeader != null) {
                var maxAge = context.Options.HstsHeader.MaxAge.TotalSeconds;
                context.HttpContext.Response.Headers[HeaderNames.Hsts] = context.Options.HstsHeader.IncludeSubDomains ?
                    $"max-age={maxAge}; includeSubDomains" : $"max-age={maxAge}";
            }
        }

        // Fall back method for GET requests
        HttpRedirectMethod Fallback(HttpRedirectMethod method) {
            switch (method) {
                case HttpRedirectMethod.TemporaryRedirect:
                    return HttpRedirectMethod.Found;
                case HttpRedirectMethod.PermanentRedirect:
                    return HttpRedirectMethod.MovedPermanently;
                default:
                    return method;
            }
        }

        // Process SslRedirectOptions.Policies
        async Task<bool> EnforcePolicies(SslRedirectContext context) {
            foreach (var policy in context.Options.Policies)
                if (await policy.Accept(context.HttpContext)) {
                    _logger.LogDebug($"{nameof(Policies.ISslPolicy)} enforced: {{type}}", policy);
                    return true;
                }

            return false;
        }

    }

}
