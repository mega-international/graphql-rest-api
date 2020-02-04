using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema
{
    public class CompatibilityList
    {
        public string DefaultSchemaFolder { get; set; }
        public Dictionary<string, string> VersionSchemaFolder { get; set; }
    }
}
