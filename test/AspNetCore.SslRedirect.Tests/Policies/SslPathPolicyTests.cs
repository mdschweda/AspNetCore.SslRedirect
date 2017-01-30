using Microsoft.AspNetCore.Http;
using Xunit;

namespace MS.AspNetCore.Ssl.Policies {

    public class SslPathPolicyTests {

        /// <summary>
        /// A path matching the pattern must be redirected.
        /// </summary>
        [Theory(DisplayName = "SslPathPolicy.ValidPathRedirected")]
        [InlineData("/Secure/Api/Help/intro.html")]
        [InlineData("/Secure/Admin/Login.html")]
        [InlineData("/Secure/About.html")]
        public void ValidPathRedirected(string path) {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = false;
            context.Request.Path = path;
            var policy = new SslPathPolicy("/Secure/**/*.html");

            var redirect = policy.Accept(context).Result;

            Assert.True(redirect);
        }

        /// <summary>
        /// A path not matching the pattern must not be redirected.
        /// </summary>
        [Theory(DisplayName = "SslPathPolicy.InvalidPathNotRedirected")]
        [InlineData("/Secure/Api/Help/header.png")]
        [InlineData("/About.html")]
        [InlineData("/Secure")]
        public void InvalidPathNotRedirected(string path) {
            var context = new DefaultHttpContext();
            context.Request.IsHttps = false;
            context.Request.Path = path;
            var policy = new SslPathPolicy("/Secure/**/*.html");

            var redirect = policy.Accept(context).Result;

            Assert.False(redirect);
        }

    }

}
