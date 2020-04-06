using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL.Schema;

using Newtonsoft.Json;

namespace Hopex.Modules.GraphQL
{
    class SchemaPathExtractor
    {
        private readonly CompatibilityList _compatibilityList;

        public SchemaPathExtractor()
        {
            _compatibilityList = JsonConvert.DeserializeObject<CompatibilityList>(Properties.Resources.compatibility_list);
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
                    version = _compatibilityList.VersionSchemaFolder.ContainsKey(hopexVersion) ? _compatibilityList.VersionSchemaFolder[hopexVersion] : _compatibilityList.DefaultSchemaFolder;
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
