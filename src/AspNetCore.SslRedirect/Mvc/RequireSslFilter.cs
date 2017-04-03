using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MS.AspNetCore.Ssl.Mvc {

    /// <summary>
    /// An <see cref="IActionFilter"/> that confirms requests are received over HTTPS and redirects
    /// them if necessary.
    /// </summary>
    internal class RequireSslFilter : IActionFilter {

        readonly SslRedirectOptions _options;
        readonly ISslRedirector _provider;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireSslFilter"/> class.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="ISslRedirector"/> providing the redirect functionality.
        /// </param>
        /// <param name="options">The redirect options.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public RequireSslFilter(ISslRedirector provider, IOptions<SslRedirectOptions> options, ILogger<RequireSslFilter> logger) {
            _provider = provider;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async void OnActionExecuting(ActionExecutingContext context) {
            if (!context.HttpContext.Request.IsHttps)
                _logger.LogInformation("Action {action} requires SSL.",
                    context.ActionDescriptor.DisplayName);

            if (await _provider.Accept(new SslRedirectContext(context.HttpContext, _options), false))
                context.Result = new NullResult();
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context) { }

    }

}
