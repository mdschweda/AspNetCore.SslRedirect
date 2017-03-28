using System.Collections.Generic;
using MS.AspNetCore.Ssl.Policies;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Provides information needed to control <see cref="SslRedirectMiddleware"/> behavior.
    /// </summary>
    public class SslRedirectOptions {

        /// <summary>
        /// Gets or sets the HTTPS port number. The default value is <c>443</c>.
        /// </summary>
        public int SslPort {
            get;
            set;
        } = 443;

        /// <summary>
        /// Gets or sets the HTTP method used for redirecting requests. The default value is
        /// <see cref="HttpRedirectMethod.TemporaryRedirect"/>.
        /// </summary>
        public HttpRedirectMethod Method {
            get;
            set;
        } = HttpRedirectMethod.TemporaryRedirect;

        /// <summary>
        /// Get or sets the HSTS header information.
        /// </summary>
        /// <value>
        /// The information used to generate HSTS headers for secure contexts or
        /// <see langword="null"/> to disable HSTS headers.
        /// </value>
        public HstsHeader HstsHeader {
            get;
            set;
        } = new HstsHeader();

        /// <summary>
        /// Gets the collection of policies for upgrading unsecured requests.
        /// </summary>
        public ICollection<ISslPolicy> Policies {
            get;
        } = new List<ISslPolicy>();

        /// <summary>
        /// Gets or sets the SslTermination Flag
        /// </summary>
        public bool AllowSslTermination
        {
            get;
            set;
        } = false;
    }

}