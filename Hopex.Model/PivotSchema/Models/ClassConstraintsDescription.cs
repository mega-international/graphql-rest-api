using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotClassConstraintsDescription
    {
        [JsonProperty("queryNode")]
        public bool? IsEntryPoint { get; set; }
    }
}
