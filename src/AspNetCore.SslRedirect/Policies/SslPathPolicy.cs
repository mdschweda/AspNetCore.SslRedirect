using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Minimatch;

namespace MS.AspNetCore.Ssl.Policies {

    /// <summary>
    /// Implements an <see cref="ISslPolicy"/> that requires requests for certain paths to use SSL.
    /// </summary>
    public class SslPathPolicy : ISslPolicy {

        string _pattern;
        Minimatcher _matcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslPathPolicy"/> class.
        /// </summary>
        public SslPathPolicy() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SslPathPolicy"/> class.
        /// </summary>
        /// <param name="pattern">The glob pattern for paths that are included by the policy.</param>
        public SslPathPolicy(string pattern) {
            Pattern = pattern;
        }

        /// <summary>
        /// Gets or sets the glob pattern for paths that are included by the policy.
        /// </summary>
        public string Pattern {
            get {
                return _pattern;
            }
            set {
                _pattern = value;
                if (!String.IsNullOrEmpty(value))
                    _matcher = new Minimatcher(_pattern, new Options { IgnoreCase = true });
            }
        }

        /// <inheritdoc/>
        public Task<bool> Accept(HttpContext context) =>
            Task.FromResult(
                !String.IsNullOrEmpty(_pattern) &&
                _matcher.IsMatch(context.Request.Path)
            );

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(SslPathPolicy)}: \"{Pattern}\"";

    }

}