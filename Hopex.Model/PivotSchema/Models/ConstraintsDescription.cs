using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{

    public class PivotConstraintsDescription
    {
        [JsonProperty("type")]
        public string PropertyType { get; set; }
        [JsonProperty("mandatory")]
        public bool? IsRequired { get; set; }
        [JsonProperty("maxLength")]
        public string MaxLength { get; set; }
        [JsonProperty("readOnly")]
        public bool? IsReadOnly { get; set; }
        [JsonProperty("filter")]
        public bool? IsFilter { get; set; }
    }
}

