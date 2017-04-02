using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using H = Microsoft.Net.Http.Headers;

namespace MS.AspNetCore.Ssl {

    /// <inheritdoc/>
    internal class SslRedirector : ISslRedirector {

        readonly SslRedirectOptions _options;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslRedirector"/> class.
        /// </summary>
        /// <param name="options">The redirect options.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public SslRedirector(IOptions<SslRedirectOptions> options, ILogger<SslRedirector> logger) {
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> Accept(HttpContext context, bool enforcePolicies = true) {
            if (_options.AllowSslTermination) {
                HandleSslTermination(context);
            }
            if (!context.Request.IsHttps && (!enforcePolicies || await EnforcePolicies(context))) {
                context.Request.Scheme = "https";
                var host = new HostString(context.Request.Host.Host, _options.SslPort);
                var builder = new UriBuilder("https", context.Request.Host.Host, _options.SslPort) {
                    Path = context.Request.PathBase + context.Request.Path
                };
                if (context.Request.QueryString.HasValue)
                    builder.Query = context.Request.QueryString.Value;

                context.Response.Headers[H.HeaderNames.Location] = builder.Uri.AbsoluteUri;

                var method = _options.Method;
                if (HttpMethods.IsGet(context.Request.Method))
                    method = Fallback(method);
                context.Response.StatusCode = (int)method;

                _logger.LogInformation("Redirecting request {path} to {host} using {code} - {method}",
                    context.Request.Path, host, (int)method, method);
                return true;
            }

            return false;
        }

        private void HandleSslTermination(HttpContext context) {
            /*if (context.Request.Headers.ContainsKey(XForwardedProtoHeaders) &&
                ((string)context.Request.Headers[XForwardedProtoHeaders])
                .Equals("https", StringComparison.OrdinalIgnoreCase)) {
                // This was forwarded from a proxy/load balancer that has 
                // performed SSL termination
                context.Request.IsHttps = true;
                _logger.LogInformation("SSL Termination detected");
            }*/
            /*if (context.Request.Headers.ContainsKey(XForwardedPort)) {
                if (Int32.TryParse(context.Request.Headers[XForwardedPort], out var sslPort)) {
                    _options.SslPort = sslPort;
                    _logger.LogInformation($"Ssl port set to {sslPort}");
                } else {
                    _logger.LogWarning(
                        $"X-Forwarded-Port header not an integer: {context.Request.Headers[XForwardedPort]}, using the default port {_options.SslPort}");
                }
            }*/
        }

        /// <inheritdoc/>
        public void AddHstsHeader(HttpContext context) {
            if (context.Request.IsHttps && _options.HstsHeader != null) {
                var maxAge = _options.HstsHeader.MaxAge.TotalSeconds;
                context.Response.Headers[HeaderNames.Hsts] = _options.HstsHeader.IncludeSubDomains ?
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
        async Task<bool> EnforcePolicies(HttpContext context) {
            foreach (var policy in _options.Policies)
                if (await policy.Accept(context)) {
                    _logger.LogDebug($"{nameof(Policies.ISslPolicy)} enforced: {{type}}", policy);
                    return true;
                }

            return false;
        }

    }

}
