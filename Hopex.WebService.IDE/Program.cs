using Mega.Has.Commons;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Hopex.WebService.IDE
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //PreloadLogger.WaitForDebugger();
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var mc = new ModuleConfiguration(args);

            var builder = Host.CreateDefaultBuilder(args)
                            .ConfigureWebHostDefaults(webBuilder =>
                            {
                                webBuilder
                                    .UseUrls(mc.ServerInstanceUrl)
                                    .UseContentRoot(mc.Folder)
                                    .UseHASInstrumentation(mc)
                                    .UseStartup<Startup>();
                            });

            return builder;
        }
    }
}
