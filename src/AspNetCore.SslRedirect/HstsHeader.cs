using System;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Represents a HSTS header.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc6797"/>
    public class HstsHeader {

        /// <summary>
        /// Gets or sets the interval during which the UA regards the host (from whom the message
        /// was received) as a known HSTS host.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc6797#section-6.1.1"/>
        public TimeSpan MaxAge {
            get;
            set;
        } = TimeSpan.FromDays(365);

        /// <summary>
        /// Gets or sets a value that signals the UA that the HSTS policy applies to this HSTS host
        /// as well as any subdomains of the host's domain name.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc6797#section-6.1.2"/>
        public bool IncludeSubDomains {
            get;
            set;
        }

        /// <summary>
        /// Duplicates the object.
        /// </summary>
        /// <returns>The duplicated object.</returns>
        internal HstsHeader Clone() =>
            new HstsHeader {
                MaxAge = MaxAge,
                IncludeSubDomains = IncludeSubDomains
            };

    }

}