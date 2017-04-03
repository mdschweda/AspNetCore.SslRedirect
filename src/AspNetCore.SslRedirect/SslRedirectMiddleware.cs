using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// A middleware for upgrading HTTP requests to HTTPS.
    /// </summary>
    public class SslRedirectMiddleware {

        readonly RequestDelegate _next;
        readonly SslRedirectOptions _options;
        readonly ISslRedirector _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslRedirectMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="provider">
        /// The <see cref="ISslRedirector"/> providing the redirect functionality.
        /// </param>
        /// <param name="options">The redirect options.</param>
        public SslRedirectMiddleware(RequestDelegate next, ISslRedirector provider, IOptions<SslRedirectOptions> options) {
            _next = next;
            _options = options.Value;
            _provider = provider;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the request.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the completion of request processing.
        /// </returns>
        public async Task Invoke(HttpContext context) {
            var sslContext = new SslRedirectContext(context, _options);
            if (await _provider.Accept(sslContext))
                return;
            _provider.AddHstsHeader(sslContext);
            await _next.Invoke(context);
        }

    }

}