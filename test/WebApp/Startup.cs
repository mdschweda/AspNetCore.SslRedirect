using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApp {

    public class Startup {

        public IConfigurationRoot Configuration {
            get;
        }

        public IHostingEnvironment Environment {
            get;
        }

        public Startup(IHostingEnvironment env) {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services) =>
            services
                .AddSslRedirect(options => {
                    options.SslPort = Environment.IsDevelopment() ? 44300 : 443;
                    options.Policies.RedirectPath("/Secure/**.html");
                })
                .AddMvc();

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            if (env.IsDevelopment())
                loggerFactory
                    .AddConsole(Configuration.GetSection("Logging"))
                    .AddDebug();

            app
                .UseSslRedirect()
                .UseStaticFiles()
                .UseMvc();
        }

    }

}
