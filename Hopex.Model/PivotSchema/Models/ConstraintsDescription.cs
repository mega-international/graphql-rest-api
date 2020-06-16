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
        public int? MaxLength { get; set; }
        [JsonProperty("readOnly")]
        public bool? IsReadOnly { get; set; }
        [JsonProperty("translatable")]
        public bool? IsTranslatable { get; set; }
        [JsonProperty("formattedText")]
        public bool? IsFormattedText { get; set; }
    }
}

