using Mega.Has.Commons;
using Mega.Has.Instrumentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;

namespace Hopex.WebService.API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileSystem>(new FileSystem());
            services.AddHASModule("C8C2793A-EF69-4674-85B7-699E53BAE72A", options => options.AuthenticationMode = AuthenticationMode.Bearer);
            var mvcBuilder = services
                      .AddControllersWithViews()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();
        }

        public void Configure(IApplicationBuilder app, IModuleConfiguration moduleConfiguration, ITraceInstrumentation traceInstrumentation)
        {
            app.UseHASModule(moduleConfiguration, traceInstrumentation);
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
