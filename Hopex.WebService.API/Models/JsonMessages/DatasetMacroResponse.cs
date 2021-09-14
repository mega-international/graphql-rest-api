using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace HAS.Modules.WebService.API.Models.JsonMessages
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

        public class DatasetColumn
        {
            [JsonProperty("id")]
            public string Id { get; internal set; }

            [JsonProperty("label")]
            public string Label { get; internal set; }
        }
    }
}
