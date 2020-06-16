using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotEntityHasProperties
    {

        [JsonProperty("properties")]
        public PivotPropertyDescription[] Properties { get; set; }
    }

    public class PivotClassDescription: PivotEntityHasProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("implementInterface")]
        public string Implements { get; set; }
        [JsonProperty("constraints")]
        public PivotClassConstraintsDescription Constraints { get; set; }
        [JsonProperty("relationships")]
        public PivotRelationshipDescription[] Relationships { get; set; }
    }
}
