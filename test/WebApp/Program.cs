using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace WebApp {

    public class Program {

        public static void Main(string[] args) =>
            new WebHostBuilder()
                .UseKestrel(options => {
                    options.UseHttps("TestCertificate.p12", "password");
                })
                .UseUrls("http://localhost:44000", "https://localhost:44300")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build()
                .Run();

    }

}
