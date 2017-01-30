using System;
using MS.AspNetCore.Ssl;

namespace Microsoft.Extensions.DependencyInjection {

    /// <summary>
    /// Extension methods for setting up SSL redirection services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class SslRedirectServiceCollectionExtensions {

        /// <summary>
        /// Adds SSL redirection services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddSslRedirect(this IServiceCollection services) =>
            AddSslRedirect(services, _ => { });

        /// <summary>
        /// Adds SSL redirection services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="options">
        /// An <see cref="Action{T}"/> to configure the provided <see cref="SslRedirectOptions"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddSslRedirect(this IServiceCollection services, Action<SslRedirectOptions> options) {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return services
                .Configure(options)
                .AddSingleton<ISslRedirector, SslRedirector>();
        }

    }

}
