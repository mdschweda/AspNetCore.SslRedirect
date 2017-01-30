namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// HTTP method used when redirecting requests.
    /// </summary>
    public enum HttpRedirectMethod {
        /// <summary>
        /// The redirection is permanent and the request method might change to <c>GET</c>.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc7231#section-6.4.2"/>
        MovedPermanently = 301,
        /// <summary>
        /// The redirection is temporary and the request method might change to <c>GET</c>.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc7231#section-6.4.3"/>
        Found = 302,
        /// <summary>
        /// The redirection is temporary and the request must not change.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc7231#section-6.4.7"/>
        TemporaryRedirect = 307,
        /// <summary>
        /// The redirection is permanent and the request must not change. This status code is
        /// experimental. Clients might not support this type of redirection.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc7238"/>
        PermanentRedirect = 308
    }

}