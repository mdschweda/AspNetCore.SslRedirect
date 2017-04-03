using System;
using Microsoft.AspNetCore.Http;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Encapsulates all information about an individual request.
    /// </summary>
    public class SslRedirectContext {

        Lazy<ForwardedHeader> _fwh;

        /// <summary>
        /// Gets the HTTP-specific information about the request.
        /// </summary>
        public HttpContext HttpContext {
            get;
            private set;
        }

        /// <summary>
        /// Gets the forward header information of the request.
        /// </summary>
        public ForwardedHeader ForwardedHeader => _fwh.Value;

        /// <summary>
        /// Gets the options for the current request.
        /// </summary>
        public SslRedirectOptions Options {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SslRedirectContext"/> class.
        /// </summary>
        /// <param name="context">The HTTP-specific information about the request.</param>
        /// <param name="options">The options for the current request.</param>
        public SslRedirectContext(HttpContext context, SslRedirectOptions options) {
            HttpContext = context;
            Options = options.Clone();
            _fwh = new Lazy<ForwardedHeader>(() => ForwardedHeader.FromHttpContext(context));
        }

    }

}
