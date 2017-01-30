using System;
using Microsoft.Extensions.DependencyInjection;
using MS.AspNetCore.Ssl;

namespace Microsoft.AspNetCore.Builder {

    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add SSL redirections to the
    /// request execution pipeline.
    /// </summary>
    public static class SslRedirectBuilderExtensions {

        /// <summary>
        /// Adds SSL redirection to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseSslRedirect(this IApplicationBuilder app) {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var provider = app.ApplicationServices.GetRequiredService<ISslRedirector>();
            app.UseMiddleware<SslRedirectMiddleware>(provider);
            return app;
        }

    }

}
