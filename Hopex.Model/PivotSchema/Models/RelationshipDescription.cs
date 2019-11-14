using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotRelationshipDescription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("pathToTarget")]
        public PivotPathDescription[] Path { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
