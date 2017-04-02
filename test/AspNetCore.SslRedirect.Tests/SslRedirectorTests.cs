using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace MS.AspNetCore.Ssl.Tests {

    public class SslRedirectorTests {

        ISslRedirector BuildRedirector() => BuildRedirector(_ => { });

        ISslRedirector BuildRedirector(Action<SslRedirectOptions> options) {
            var services = new ServiceCollection();
            return services
                .AddOptions()
                .AddSslRedirect(options)
                .AddTransient(typeof(ILogger<>), typeof(LoggerStub<>))
                .BuildServiceProvider()
                .GetService<ISslRedirector>();
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must be set for HTTPS requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderAdded")]
        public void HstsHeaderAdded() {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = true;

            BuildRedirector().AddHstsHeader(context);

            Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must not be set for HTTP requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderOmitted")]
        public void HstsHeaderOmitted() {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = false;

            BuildRedirector().AddHstsHeader(context);

            Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader"/> must not be set when set to <see langword="null"/>.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderOmittedWhenNull")]
        public void HstsHeaderOmittedWhenNull() {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = true;

            BuildRedirector(o => o.HstsHeader = null).AddHstsHeader(context);

            Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
        }

        /// <summary>
        /// <see cref="HstsHeader.IncludeSubDomains"/> must be taken into account.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderIncludesSubdomains")]
        public void HstsHeaderIncludesSubdomains() {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = true;

            BuildRedirector(o => o.HstsHeader.IncludeSubDomains = true)
                .AddHstsHeader(context);

            Assert.True(
                ((string)context.Response.Headers["Strict-Transport-Security"])
                    .EndsWith("includeSubDomains")
            );
        }

        /// <summary>
        /// Configured <see cref="HstsHeader.MaxAge"/> value must be used.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.HstsHeaderMaxAgeSet")]
        public void HstsHeaderMaxAgeSet() {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = true;

            BuildRedirector(o => o.HstsHeader.MaxAge = TimeSpan.FromSeconds(123))
                .AddHstsHeader(context);

            Assert.True(
                ((string)context.Response.Headers["Strict-Transport-Security"])
                    .StartsWith("max-age=123")
            );
        }

        /// <summary>
        /// HTTPS request must not be redirected.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.SecureRequestNotRedirected")]
        public void SecureRequestNotRedirected() {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = true;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var redirect = BuildRedirector(o => o.Policies.RedirectAll())
                .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// HTTP request must be redirected.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.UnsecureRequestRedirected")]
        public void UnsecureRequestRedirected() {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var redirect = BuildRedirector(o => o.Policies.RedirectAll())
                .Accept(context).Result;

            Assert.False(context.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Configured <see cref="SslRedirectOptions.Method"/> must be used.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.RedirectMethodSet")]
        public void RedirectMethodSet() {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.PermanentRedirect;
            })
            .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpRedirectMethod.PermanentRedirect);
        }

        /// <summary>
        /// <see cref="HttpRedirectMethod"/> must be set to fallback value for GET requests.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.RedirectMethodFallsBack")]
        public void RedirectMethodFallsBack() {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Get;
            context.Request.IsHttps = false;

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.TemporaryRedirect;
            })
            .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpRedirectMethod.Found);
        }

        /// <summary>
        /// Response <see cref="HeaderNames.Location"/> header must be set to correct value.
        /// </summary>
        [Fact(DisplayName = "SslRedirector.LocationSet")]
        public void LocationSet() {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Path = "/test";
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;

            var redirect = BuildRedirector(o => {
                o.SslPort = 1234;
                o.Policies.RedirectAll();
                o.Method = HttpRedirectMethod.PermanentRedirect;
            })
            .Accept(context).Result;

            Assert.True(context.Response.Headers[HeaderNames.Location] == "https://localhost:1234/test");
        }

        [Fact(DisplayName = "SslRedirector.SslTerminationNoRedirect")]
        public void SslTerminationDoesNotRedirectForSecureForward()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;
            context.Request.Headers["X-Forwarded-Proto"] = "https";

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.AllowSslTermination = true;
            })
            .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact(DisplayName = "SslRedirector.SslTerminationRedirected")]
        public void SslTerminationRedirectsIfNotSecureForward()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;
            context.Request.Headers["X-Forwarded-Proto"] = "http";

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.AllowSslTermination = true;
            })
            .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpRedirectMethod.TemporaryRedirect);
        }

        [Fact(DisplayName = "SslRedirector.SslTerminationRedirectedOnNoHeader")]
        public void SslTerminationRedirectsIfXForwardedProtoHeaderNotFound()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.AllowSslTermination = true;
            })
            .Accept(context).Result;

            Assert.True(context.Response.StatusCode == (int)HttpRedirectMethod.TemporaryRedirect);
        }

        [Fact(DisplayName = "SslRedirector.ForwardedPortSetOnRedirection")]
        public void ProxySSLTerminationSetsForwardedPort()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("localhost", 80);
            context.Request.Method = HttpMethods.Post;
            context.Request.IsHttps = false;
            context.Request.Path = "/test";
            context.Request.Headers["X-Forwarded-Proto"] = "http";
            context.Request.Headers["X-Forwarded-Port"] = "1234";

            var redirect = BuildRedirector(o => {
                o.Policies.RedirectAll();
                o.AllowSslTermination = true;
            })
            .Accept(context).Result;

            Assert.True(context.Response.Headers[HeaderNames.Location] == "https://localhost:1234/test");
        }
    }

}
