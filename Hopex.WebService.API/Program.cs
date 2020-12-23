using Mega.Has.Commons;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Hopex.WebService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
