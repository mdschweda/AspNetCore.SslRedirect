using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using static System.StringSplitOptions;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Encapsulates forward header information.
    /// </summary>
    public class ForwardedHeader {

        static readonly IReadOnlyDictionary<string, string> _xProtocolHeaders =
            new Dictionary<string, string> {
                ["X-Forwarded-Proto"] = "https",
                ["Front-End-Https"] = "on",
                ["X-Forwarded-Protocol"] = "https",
                ["X-Forwarded-Ssl"] = "on",
                ["X-Url-Scheme"] = "https"
            };

        /// <summary>
        /// Gets the original protocol the client used.
        /// </summary>
        public ProtocolType Protocol {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of the originating IP addresses of a request.
        /// </summary>
        /// <remarks>
        /// Each additional entry represents an intermediary that relayed the request.
        /// </remarks>
        public IReadOnlyCollection<string> For {
            get;
            private set;
        } = new string[0];

        /// <summary>
        /// Gets the the original host name requested by the client.
        /// </summary>
        public string Host {
            get;
            private set;
        }

        /// <summary>
        /// Gets the the the interface where the request came in to the proxy server.
        /// </summary>
        public string By {
            get;
            private set;
        }

        /// <summary>
        /// Gets the the original port the client used to connect.
        /// </summary>
        public int? Port {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a <see cref="ForwardedHeader"/> object from a <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context">The context to parse.</param>
        /// <returns>The constructed <see cref="ForwardedHeader"/> object or <see langword="null"/>.</returns>
        public static ForwardedHeader FromHttpContext(HttpContext context) {
            var headers = context?.Request.Headers ??
                throw new ArgumentNullException(nameof(context));

            ForwardedHeader result = null;
            ForwardedHeader header() {
                if (result == null)
                    result = new ForwardedHeader();
                return result;
            }

            foreach (var pair in _xProtocolHeaders)
                if (headers.ContainsKey(pair.Key))
                    header().Protocol =
                        pair.Value.Equals(headers[pair.Key], StringComparison.OrdinalIgnoreCase) ?
                            ProtocolType.Https : ProtocolType.Http;

            if (headers.TryGetValue(HeaderNames.ForwardedFor, out var @for))
                header().For = @for
                    .SelectMany(f => f.Split(new[] { ',' }, RemoveEmptyEntries))
                    .Select(f => f.Trim())
                    .ToArray();
            else if (headers.TryGetValue(HeaderNames.ProxyUserIp, out var @ip))
                header().For = @ip;

            if (headers.TryGetValue(HeaderNames.ForwardedHost, out var host))
                header().Host = host;

            if (headers.TryGetValue(HeaderNames.ForwardedPort, out var pv))
                if (Int32.TryParse(pv, out var port))
                    header().Port = port;

            // https://tools.ietf.org/html/rfc7239
            if (headers.TryGetValue(HeaderNames.Forwarded, out var fw)) {
                var values = fw
                    .SelectMany(f => f.Split(new[] { ';', ',' }, RemoveEmptyEntries))
                    .Select(f => f.Split(new[] { '=' }, RemoveEmptyEntries))
                    .Where(f => f.Length == 2)
                    .ToLookup(f => f[0].Trim().ToUpperInvariant(), f => f[1].Trim());

                if (values.Contains("PROTO"))
                    header().Protocol = values["PROTO"].Last().Equals("https", StringComparison.OrdinalIgnoreCase) ?
                        ProtocolType.Https : ProtocolType.Http;

                if (values.Contains("FOR"))
                    header().For = values["FOR"].ToArray();

                if (values.Contains("HOST"))
                    header().Host = values["HOST"].Last();

                if (values.Contains("BY"))
                    header().By = values["BY"].Last();
            }

            return result;
        }

    }

}
