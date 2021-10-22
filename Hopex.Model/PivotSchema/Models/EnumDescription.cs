using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{

    public class PivotEnumDescription
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("internalValue")]
        public object InternalValue { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
    }
}
