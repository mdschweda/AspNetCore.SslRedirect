using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MS.AspNetCore.Ssl.Policies {

    /// <summary>
    /// Implements an <see cref="ISslPolicy"/> that requires every request to use SSL.
    /// </summary>
    public class SslAllPolicy : ISslPolicy {

        /// <inheritdoc/>
        public Task<bool> Accept(HttpContext context) => Task.FromResult(true);

    }

}