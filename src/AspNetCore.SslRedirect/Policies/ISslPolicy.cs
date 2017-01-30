using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MS.AspNetCore.Ssl.Policies {

    /// <summary>
    /// Represents a policy that determines which requests require SSL.
    /// </summary>
    public interface ISslPolicy {

        /// <summary>
        /// Enforces the policy by inspecting the unsecured request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the request.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the completion of request processing. A result
        /// of <see langword="true"/> will signal the <see cref="SslRedirectMiddleware"/> to
        /// upgrade the request.
        /// </returns>
        Task<bool> Accept(HttpContext context);

    }

}