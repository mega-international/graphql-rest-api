using System;
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
        /// Version of the schema
        /// </summary>
        [JsonProperty("version")]
        public Version Version { get; set; }

        /// <summary>
        /// List of class descriptions
        /// </summary>
        [JsonProperty("metaclass")]
        public PivotClassDescription[] Classes { get; set; }

        [JsonProperty("interfaces")]
        public PivotClassDescription[] Interfaces { get; set; }

        /// <summary>
        /// Used for schema inheritence
        /// </summary>
        [JsonProperty("extends")]
        public string OverrideSchema { get; set; }
    }

    public class Version
    {
        [JsonProperty("jsonVersion")]
        public string JsonVersion { get; set; }
        [JsonProperty("platformVersion")]
        public string PlatformVersion { get; set; }
        [JsonProperty("metamodelVersion")]
        public string MetamodelVersion { get; set; }
        [JsonProperty("latestGeneration")]
        public DateTime LatestGeneration { get; set; }
    }
}
