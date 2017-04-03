using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MS.AspNetCore.Ssl.Mvc {

    /// <summary>
    /// Confirms that requests are received over HTTPS and redirects them if necessary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireSslAttribute : Attribute, IFilterFactory {

        /// <inheritdoc/>
        public bool IsReusable => true;

        /// <inheritdoc/>
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) {
            var provider = serviceProvider.GetRequiredService<ISslRedirector>();
            var options = serviceProvider.GetRequiredService<IOptions<SslRedirectOptions>>();
            var logger = serviceProvider.GetRequiredService<ILogger<RequireSslFilter>>();
            return new RequireSslFilter(provider, options, logger);
        }

    }

}