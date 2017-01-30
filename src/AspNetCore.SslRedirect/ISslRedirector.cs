using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MS.AspNetCore.Ssl {

    /// <summary>
    /// Provides the functionality for redirecting unsecure requests to HTTPS.
    /// </summary>
    public interface ISslRedirector {

        /// <summary>
        /// Inspect and upgrade the request if necessary.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the request.</param>
        /// <param name="enforcePolicies">
        /// A value that indicates if <see cref="Policies.ISslPolicy">policies</see> are taken
        /// into account.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that that will complete with a result of <see langword="true"/>
        /// if the request was redirected, otherwise with a result of <see langword="false"/>.
        /// </returns>
        Task<bool> Accept(HttpContext context, bool enforcePolicies = true);

        /// <summary>
        /// Adds a <see cref="HstsHeader"/> to the response headers.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the request.</param>
        void AddHstsHeader(HttpContext context);

    }

}
