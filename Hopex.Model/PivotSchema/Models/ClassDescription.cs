using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotClassDescription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("queryNode")]
        public bool? IsEntryPoint { get; set; }
        [JsonProperty("properties")]
        public PivotPropertyDescription[] Properties { get; set; }
        [JsonProperty("relationships")]
        public PivotRelationshipDescription[] Relationships { get; set; }
    }
}
