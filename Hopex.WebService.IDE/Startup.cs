using Hopex.WebService.IDE.Models;
using Mega.Has.Commons;
using Mega.Has.Instrumentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopex.WebService.IDE
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationSetting<HopexGraphQlSettings>("Hopex supervision settings");

            services.AddHASModule("6CAE27E7-FF79-4D33-A49E-6553CC766481", options => options.AuthenticationMode = AuthenticationMode.HopexSession);
            var mvcBuilder = services
                .AddControllersWithViews();

            

#if (DEBUG)
            mvcBuilder.AddRazorRuntimeCompilation();
#endif
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
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
