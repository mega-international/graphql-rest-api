using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL.Schema;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hopex.Modules.GraphQL
{
    class SchemaPathExtractor
    {
        private readonly CompatibilityList _compatibilityList;

        public SchemaPathExtractor()
        {
            var compatibilityListFile = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CONFIG\\compatibility_list.json";
            _compatibilityList = JsonConvert.DeserializeObject<CompatibilityList>(File.ReadAllText(compatibilityListFile));
        }

        internal SchemaReference Extract(IMegaRoot iRoot, string requestPath, string webServiceRoute, string environmentId)
        {
            string version;
            string schemaName;

            var userRequest = requestPath.Substring($"/api/{webServiceRoute}/".Length).Split('/');
            switch (userRequest.Length)
            {
                case 1:
                    var hopexVersion = iRoot.CurrentEnvironment.Site.VersionInformation.Name;
                    string FindVersion()
                    {
                        if (_compatibilityList.VersionSchemaFolder.ContainsKey(hopexVersion))
                        {
                            return _compatibilityList.VersionSchemaFolder[hopexVersion];
                        }
                        var key = _compatibilityList.VersionSchemaFolder.Keys.FirstOrDefault(x => Regex.IsMatch(hopexVersion, Utils.WildCardToRegular(x)));
                        return key != null ? _compatibilityList.VersionSchemaFolder[key] : _compatibilityList.DefaultSchemaFolder;
                    }
                    version = FindVersion();
                    schemaName = userRequest[0];
                    break;
                case 2:
                    version = userRequest[0];
                    schemaName = userRequest[1];
                    break;
                default:
                    return null;
            }

            return new SchemaReference { SchemaName = schemaName, Version = version, EnvironmentId = environmentId };
        }
    }
}
