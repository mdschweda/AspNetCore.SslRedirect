namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// A HTTP protocol type.
    /// </summary>
    public enum ProtocolType {
        /// <summary>
        /// No protocol type was specified.
        /// </summary>
        Unspecified,
        /// <summary>
        /// HTTP protocol.
        /// </summary>
        Http,
        /// <summary>
        /// HTTP with TLS.
        /// </summary>
        Https
    }

}
