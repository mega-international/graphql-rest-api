using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotPropertyDescription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("constraints")]
        public PivotConstraintsDescription Constraints { get; set; }
        [JsonProperty("enumValues")]
        public PivotEnumDescription[] EnumValues { get; set; }

        [JsonProperty("setterFormat")]
        public string SetterFormat { get; set; }
        [JsonProperty("getterFormat")]
        public string GetterFormat { get; set; }
    }
}

