
using Hopex.ApplicationServer.WebServices;

using System;
using System.IO;

namespace Hopex.Modules.GraphQL
{
    public static class ContextUtils
    {
        public static bool IsRunningInHAS => Environment.GetEnvironmentVariable("HOPEX_INSTANCE_DIRECTORY") != null;
        public static string HASCustomSchemasFolder => Path.Combine(Environment.GetEnvironmentVariable("HOPEX_CUSTOM_DIRECTORY"), "Schemas");

        public static string GetEnvironmentId(IHopexContext hopexContext)
        {
            if (hopexContext.Request.Headers.ContainsKey("EnvironmentId"))
            {
                if (hopexContext.Request.Headers["EnvironmentId"].Length > 0)
                {
                    return hopexContext.Request.Headers["EnvironmentId"][0];
                }
            }
            return string.Empty;
        }
    }
}
