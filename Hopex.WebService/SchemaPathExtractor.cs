using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL.Schema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hopex.Modules.GraphQL
{
    class SchemaPathExtractor
    {
        internal string Extract(IMegaRoot iRoot, string requestPath, string environmentId, string webServiceRoute, ILogger logger)
        {
            var configFolder = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\CONFIG";
            string version;
            string schemaName;

            var userRequest = requestPath.Substring($"/api/{webServiceRoute}/".Length).Split('/');
            switch (userRequest.Length)
            {
                case 1:
                    var compatibilityListJson = File.ReadAllText($@"{configFolder}\compatibility_list.json");
                    var compatibilityList = JsonConvert.DeserializeObject<CompatibilityList>(compatibilityListJson);
                    var hopexVersion = iRoot.CurrentEnvironment.Site.VersionInformation.Name;
                    version = compatibilityList.VersionSchemaFolder.ContainsKey(hopexVersion) ? compatibilityList.VersionSchemaFolder[hopexVersion] : compatibilityList.DefaultSchemaFolder;
                    schemaName = userRequest[0];
                    break;
                case 2:
                    version = userRequest[0];
                    schemaName = userRequest[1];
                    break;
                default:
                    return "";
            }

            if (Directory.Exists($"{configFolder}\\{version}\\Custom"))
            {
                var customFiles = Directory.GetFiles($"{configFolder}\\{version}\\Custom\\", $"*{schemaName}.json");
                if (customFiles.Any())
                {
                    if (customFiles.Contains($"{configFolder}\\{version}\\Custom\\{environmentId}_{schemaName}.json"))
                    {
                        return $"{configFolder}\\{version}\\Custom\\{environmentId}_{schemaName}";
                    }
                    if (customFiles.Contains($"{configFolder}\\{version}\\Custom\\{schemaName}.json"))
                    {
                        return $"{configFolder}\\{version}\\Custom\\{schemaName}";
                    }
                    var dotNetMacroFolder = Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName).FullName;
                    configFolder = configFolder.Replace(dotNetMacroFolder, "");
                    var ex = new Exception($"Schema \"{configFolder}\\{version}\\Custom\\{environmentId}_{schemaName}.json\" or \"{configFolder}\\{version}\\Custom\\{schemaName}.json\" not found.");
                    logger.LogError(ex);
                    throw ex;
                }
            }

            return $"{configFolder}\\{version}\\Standard\\{schemaName}";
        }
    }
}
