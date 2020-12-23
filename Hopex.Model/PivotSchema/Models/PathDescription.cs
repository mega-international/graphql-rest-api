using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotPathDescription : PivotEntityHasProperties
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("maeName")]
        public string RoleName { get; set; }

        [JsonProperty("maeID")]
        public string RoleId { get; set; }

        [JsonProperty("metaClassName")]
        public string MetaClassName { get; set; }

        [JsonProperty("metaClassID")]
        public string MetaClassId { get; set; }

        [JsonProperty("multiplicity")]
        public string Multiplicity { get; set; }

        [JsonProperty("condition")]
        public PivotPathConditionDescription Condition { get; set; }
    }
}
