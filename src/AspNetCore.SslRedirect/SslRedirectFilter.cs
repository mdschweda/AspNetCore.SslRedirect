namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Represents a function that wraps SSL redirection.
    /// </summary>
    /// <param name="context">The <see cref="SslRedirectContext"/> of the request.</param>
    /// <returns><see langword="true"/>to terminate SSL usage; otherwise <see langword="false"/>.</returns>
    public delegate bool SslRedirectFilter(SslRedirectContext context);
    
}
