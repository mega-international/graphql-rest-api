using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{

    public class PivotEnumDescription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("internalValue")]
        public string InternalValue { get; set; }
    }
}
