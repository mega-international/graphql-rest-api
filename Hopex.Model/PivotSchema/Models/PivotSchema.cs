using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotSchema
    {
        /// <summary>
        /// Unique schema name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// List of class descriptions
        /// </summary>
        [JsonProperty("metaclass")]
        public PivotClassDescription[] Classes { get; set; }

        /// <summary>
        /// Used for schema inheritence
        /// </summary>
        [JsonProperty("inherits")]
        public string OverrideSchema { get; set; }
    }
}
