using System;
using System.Threading.Tasks;
using Mega.Has.Commons;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hopex.WebService.IDE
{
    public class Program
    {
        private static ModuleConfiguration _moduleConfiguration;

        public static async Task Main(string[] args)
        {
            try
            {
                // For debugging in visual studio
                // Start this project directly with a --attach-to=<has-address> argument
                // has-address must be a local running HAS instance addres
                _moduleConfiguration = await ModuleConfiguration.CreateAsync(args);
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                PreloadLogger.LogError("UAS -" + ex.Message);
                Log.CloseAndFlush();
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //PreloadLogger.WaitForDebugger();

            var mc = _moduleConfiguration ?? new ModuleConfiguration(args);

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseHASInstrumentation(mc)
                        .UseUrls(mc.ServerInstanceUrl)
                        .UseContentRoot(mc.Folder)
                        .UseKestrel((options) =>
                        {
                            // Do not add the Server HTTP header.
                            options.AddServerHeader = false;
                        })
                        .UseStartup<Startup>();
                });

            return builder;
        }
    }
}
