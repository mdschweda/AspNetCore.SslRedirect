using System;
using System.Collections.Generic;
using MS.AspNetCore.Ssl.Policies;

namespace Microsoft.Extensions.DependencyInjection {

    /// <summary>
    /// Extension methods for adding <see cref="ISslPolicy"/> instances to an <see cref="ICollection{T}" />.
    /// </summary>
    public static class SslRedirectPolicyExtensions {

        /// <summary>
        /// Adds a <see cref="SslPathPolicy"/> to the collection.
        /// </summary>
        /// <param name="policies">The <see cref="ISslPolicy"/> collection to add the policy to.</param>
        /// <param name="pattern">The glob pattern for paths that are included by the policy.</param>
        /// <returns>
        /// The <see cref="ICollection{T}"/> so that additional calls can be chained.
        /// </returns>
        public static ICollection<ISslPolicy> RedirectPath(this ICollection<ISslPolicy> policies, string pattern) {
            if (policies == null)
                throw new ArgumentNullException(nameof(policies));

            policies.Add(new SslPathPolicy(pattern));
            return policies;
        }

        /// <summary>
        /// Adds a <see cref="SslAllPolicy"/> to the collection.
        /// </summary>
        /// <param name="policies">The <see cref="ISslPolicy"/> collection to add the policy to.</param>
        /// <returns>
        /// The <see cref="ICollection{T}"/> so that additional calls can be chained.
        /// </returns>
        public static ICollection<ISslPolicy> RedirectAll(this ICollection<ISslPolicy> policies) {
            if (policies == null)
                throw new ArgumentNullException(nameof(policies));

            policies.Add(new SslAllPolicy());
            return policies;
        }

    }

}
