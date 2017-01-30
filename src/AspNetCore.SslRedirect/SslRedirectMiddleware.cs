using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// A middleware for upgrading HTTP requests to HTTPS.
    /// </summary>
    public class SslRedirectMiddleware {

        readonly RequestDelegate _next;
        readonly ISslRedirector _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslRedirectMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="provider">
        /// The <see cref="ISslRedirector"/> providing the redirect functionality.
        /// </param>
        public SslRedirectMiddleware(RequestDelegate next, ISslRedirector provider) {
            _next = next;
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
            if (await _provider.Accept(context))
                return;
            _provider.AddHstsHeader(context);
            await _next.Invoke(context);
        }

    }

}