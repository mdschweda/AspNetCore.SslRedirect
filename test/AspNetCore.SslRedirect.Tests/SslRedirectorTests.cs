using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace MS.AspNetCore.Ssl.Tests {

    public class SslRedirectorTests {

        IServiceProvider BuildServiceProvider() => BuildServiceProvider(_ => { });

        IServiceProvider BuildServiceProvider(Action<SslRedirectOptions> options) =>
            new ServiceCollection()
                .AddOptions()
                .AddSslRedirect(options)
                .AddTransient(typeof(ILogger<>), typeof(LoggerStub<>))
                .BuildServiceProvider();

        (ISslRedirector redirector, SslRedirectContext context) Arrange() => Arrange(_ => { });

        (ISslRedirector redirector, SslRedirectContext context) Arrange(Action<SslRedirectOptions> options) {
            var services = BuildServiceProvider(options);
            var redirectOptions = services.GetRequiredService<IOptions<SslRedirectOptions>>();
            var context = new SslRedirectContext(new DefaultHttpContext(), redirectOptions.Value);
            var redirector = services.GetRequiredService<ISslRedirector>();
            return (redirector, context);
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must be set for HTTPS requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderAdded")]
        public void HstsHeaderAdded() {
            (var redirector, var context) = Arrange();
            context.HttpContext.Request.IsHttps = true;

            redirector.AddHstsHeader(context);

            Assert.True(context.HttpContext.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must not be set for HTTP requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderOmitted")]
        public void HstsHeaderOmitted() {
            (var redirector, var context) = Arrange();
            context.HttpContext.Request.IsHttps = false;

            redirector.AddHstsHeader(context);

            Assert.False(context.HttpContext.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must not be set when set to <see langword="null"/>.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderOmittedWhenNull")]
        public void HstsHeaderOmittedWhenNull() {
            (var redirector, var context) = Arrange(o => o.HstsHeader = null);
            context.HttpContext.Request.IsHttps = true;

            redirector.AddHstsHeader(context);

            Assert.False(context.HttpContext.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader.IncludeSubDomains"/> must be taken into account.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderIncludesSubdomains")]
        public void HstsHeaderIncludesSubdomains() {
            (var redirector, var context) = Arrange(o => o.HstsHeader.IncludeSubDomains = true);
            context.HttpContext.Request.IsHttps = true;

            redirector.AddHstsHeader(context);

            Assert.True(
                ((string)context.HttpContext.Response.Headers["Strict-Transport-Security"])
                    .EndsWith("includeSubDomains")
            );
        }

        /// <summary>
        /// Configured <see cref="HstsHeader.MaxAge"/> value must be used.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderMaxAgeSet")]
        public void HstsHeaderMaxAgeSet() {
            (var redirector, var context) = Arrange(o => o.HstsHeader.MaxAge = TimeSpan.FromSeconds(123));
            context.HttpContext.Request.IsHttps = true;

            redirector.AddHstsHeader(context);

            Assert.True(
                ((string)context.HttpContext.Response.Headers["Strict-Transport-Security"])
                    .StartsWith("max-age=123")
            );
        }

        /// <summary>
        /// HTTPS request must not be redirected.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.SecureRequestNotRedirected")]
        public void SecureRequestNotRedirected() {
            (var redirector, var context) = Arrange(o => o.Policies.RedirectAll());
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = true;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// HTTP request must be redirected.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.UnsecureRequestRedirected")]
        public void UnsecureRequestRedirected() {
            (var redirector, var context) = Arrange(o => o.Policies.RedirectAll());
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = redirector.Accept(context).Result;

            Assert.False(context.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Configured <see cref="SslRedirectOptions.Method"/> must be used.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.RedirectMethodSet")]
        public void RedirectMethodSet() {
            (var redirector, var context) = Arrange(o => {
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.PermanentRedirect;
            });
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.StatusCode == (int)HttpRedirectMethod.PermanentRedirect);
        }

        /// <summary>
        /// <see cref="HttpRedirectMethod"/> must be set to fallback value for GET requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.RedirectMethodFallsBack")]
        public void RedirectMethodFallsBack() {
            (var redirector, var context) = Arrange(o => {
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.TemporaryRedirect;
            });
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Get;
            context.HttpContext.Request.IsHttps = false;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.StatusCode == (int)HttpRedirectMethod.Found);
        }

        /// <summary>
        /// Response <see cref="HeaderNames.Location"/> header must be set to correct value.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.LocationSet")]
        public void LocationSet() {
            (var redirector, var context) = Arrange(o => {
                o.SslPort = 1234;
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.PermanentRedirect;
            });
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Path = "/test";
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.Headers[HeaderNames.Location] == "https://localhost:1234/test");
        }

        /// <summary>
        /// Filter delegate must terminate redirection.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.FilterTerminatesRedirection")]
        public void FilterTerminatesRedirection() {
            (var redirector, var context) = Arrange(o => {
                o.Policies.RedirectAll();
                o.Filter = _ => true;
            });
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Filter must use per-request of options.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.FilterDuplicatesOptions")]
        public void FilterDuplicatesOptions() {
            (var redirector, var context) = Arrange(o => {
                o.Policies.RedirectAll();
                o.Filter = c => {
                    c.Options.Policies.Clear();
                    return false;
                };
            });
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = redirector.Accept(context).Result;

            Assert.True(context.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Filter gets forwarded header information passed in.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.FilterReceivesForwardedHeader")]
        public void FilterReceivesForwardedHeader() {
            ForwardedHeader header = null;

            (var redirector, var context) = Arrange(o => {
                o.Policies.RedirectAll();
                o.Filter = c => {
                    header = c.ForwardedHeader;
                    return false;
                };
            });
            context.HttpContext.Request.Headers["Forwarded"] = "proto=https";
            context.HttpContext.Request.Host = new HostString("localhost", 80);
            context.HttpContext.Request.Method = HttpMethods.Post;
            context.HttpContext.Request.IsHttps = false;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = redirector.Accept(context).Result;

            Assert.NotNull(header);
        }

    }

}
