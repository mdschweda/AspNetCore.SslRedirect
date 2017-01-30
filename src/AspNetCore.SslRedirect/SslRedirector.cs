using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MS.AspNetCore.Ssl {

    /// <inheritdoc/>
    internal class SslRedirector : ISslRedirector {

        /// <summary>
        /// The name of HSTS headers.
        /// </summary>
        internal const string HstsHeader = "Strict-Transport-Security";

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
            if (!context.Request.IsHttps && (!enforcePolicies || await EnforcePolicies(context))) {
                context.Request.Scheme = "https";
                var host = new HostString(context.Request.Host.Host, _options.SslPort);

                context.Response.Headers[HeaderNames.Location] =
                    $"https://{host}{context.Request.Path}";

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

        /// <inheritdoc/>
        public void AddHstsHeader(HttpContext context) {
            if (context.Request.IsHttps && _options.HstsHeader != null) {
                var maxAge = _options.HstsHeader.MaxAge.TotalSeconds;
                context.Response.Headers[HstsHeader] = _options.HstsHeader.IncludeSubDomains ?
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
