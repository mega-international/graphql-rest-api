using System;
using Newtonsoft.Json;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class MetaModelVersion
    {
        [JsonProperty("compiled")]
        public bool Compiled { get; set; }

        [JsonProperty("loaded")]
        public bool Loaded { get; set; }

        [JsonProperty("technical")]
        public bool Technical { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
