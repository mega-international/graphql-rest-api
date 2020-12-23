using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Hopex.Modules.GraphQL.Dataset
{
    public class DatasetMacroResponse
    {
        [JsonProperty("header")]
        public DatasetHeader Header { get; private set; } = new DatasetHeader();

        [JsonProperty("data")]
        public List<ExpandoObject> Data { get; private set; } = new List<ExpandoObject>();

        public class DatasetHeader
        {
            [JsonProperty("columns")]
            public List<DatasetColumn> Columns { get; private set; } = new List<DatasetColumn>();

        }
    }
}
