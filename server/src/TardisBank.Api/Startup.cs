using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Westwind.AspNetCore.LiveReload;
using System.IO;

namespace TardisBank.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddLiveReload(config => {
                config.FolderToMonitor = Path.GetFullPath("..");
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var appConfiguration = AppConfiguration.LoadFromEnvironment();

            app.UseErrorHandler(env.IsDevelopment());

            app.UseLiveReload();
            app.Use(Authentication.Authenticate(
                token => Authentication.DecryptToken(
                    appConfiguration.EncryptionKey, 
                    () => DateTimeOffset.Now, 
                    token)));

            app.UseRouter(new RouteBuilder(app).CreateRoutes(appConfiguration).Build());
        }
    }
}
