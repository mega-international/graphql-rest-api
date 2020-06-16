using Hopex.ApplicationServer.WebServices;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Hopex.Modules.GraphQL
{
    public static class Utils
    {
        public static bool IsRunningInHAS => Environment.GetEnvironmentVariable("HOPEX_INSTANCE_DIRECTORY") != null;
        public static string HASCustomSchemasFolder => Path.Combine(Environment.GetEnvironmentVariable("HOPEX_CUSTOM_RESOURCE_DIRECTORY"), "Schemas");

        public static string GetEnvironmentId(IHopexContext hopexContext)
        {
            if (hopexContext.Request.Headers.ContainsKey("x-hopex-environment-id"))
            {
                if (hopexContext.Request.Headers["x-hopex-environment-id"].Length > 0)
                {
                    return hopexContext.Request.Headers["x-hopex-environment-id"][0];
                }
            }
            return string.Empty;
        }

        public static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$"; 
        }
    }
}
