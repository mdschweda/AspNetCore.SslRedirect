using Microsoft.AspNetCore.Http;
using Xunit;

namespace MS.AspNetCore.Ssl.Tests {

    public class ForwardedHeaderTests {

        /// <summary>
        /// <see cref="ForwardedHeader.FromHttpContext(HttpContext)"/> must return <see langword="null"/>
        /// when no Forwarded headers are present.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.ParseReturnsNull")]
        public void ParseReturnsNull() {
            var context = new DefaultHttpContext();

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Null(header);
        }

        /// <summary>
        /// <see cref="ForwardedHeader.Protocol"/> must be set for HTTP requests with applicable Forwarded headers.
        /// </summary>
        [Theory(DisplayName = "ForwardedHeader.XForwardedProtocolRead")]
        [InlineData("X-Forwarded-Proto", "https")]
        [InlineData("Front-End-Https", "on")]
        [InlineData("X-Forwarded-Protocol", "https")]
        [InlineData("X-Forwarded-Ssl", "on")]
        [InlineData("X-Url-Scheme", "https")]
        public void XForwardedProtocolRead(string key, string value) {
            var context = new DefaultHttpContext();
            context.Request.Headers[key] = value;

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.Protocol, ProtocolType.Https);
        }

        /// <summary>
        /// <see cref="ForwardedHeader.For"/> must be set for HTTP requests with <c>X-Forwarded-For</c> headers.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.XForwardedForParsed")]
        public void XForwardedForParsed() {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-for"] = "172.217.20.195, fd00::211:32ff:fe0c:9319, clientA";

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.For.Count, 3);
            Assert.Contains(header.For, f => f == "172.217.20.195");
            Assert.Contains(header.For, f => f == "fd00::211:32ff:fe0c:9319");
            Assert.Contains(header.For, f => f == "clientA");
        }

        /// <summary>
        /// <see cref="ForwardedHeader.For"/> must be set for HTTP requests with <c>X-ProxyUser-Ip</c> headers.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.XProxyUserIpRead")]
        public void XProxyUserIpRead() {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-ProxyUser-Ip"] = "172.217.20.195";

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.For.Count, 1);
            Assert.Contains(header.For, f => f == "172.217.20.195");
        }

        /// <summary>
        /// <see cref="ForwardedHeader.Host"/> must be set for HTTP requests with <c>X-Forwarded-Host</c> headers.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.XForwardedHostRead")]
        public void XForwardedHostRead() {
            var host = "myhost";
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-Host"] = host;

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.Host, host);
        }

        /// <summary>
        /// <see cref="ForwardedHeader.Port"/> must be set for HTTP requests with <c>X-Forwarded-Port</c> headers.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.XForwardedPortRead")]
        public void XForwardedPortRead() {
            var port = 8081;
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-Port"] = port.ToString();

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.Port, port);
        }

        /// <summary>
        /// <see cref="ForwardedHeader"/> must be constructed for HTTP requests with <c>Forwarded</c> headers.
        /// </summary>
        [Fact(DisplayName = "ForwardedHeader.XForwardedParsed")]
        public void XForwardedParsed() {
            var context = new DefaultHttpContext();
            context.Request.Headers["Forwarded"] = " for=192.0.2.43  , for =198.51.100.17; " +
                "by = 203.0.113.60 ; proto=https;host= example.com ";

            var header = ForwardedHeader.FromHttpContext(context);

            Assert.Equal(header.For.Count, 2);
            Assert.Contains(header.For, f => f == "192.0.2.43");
            Assert.Contains(header.For, f => f == "198.51.100.17");
            Assert.Equal(header.By, "203.0.113.60");
            Assert.Equal(header.Protocol, ProtocolType.Https);
            Assert.Equal(header.Host, "example.com");
        }

    }

}
