using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Models
{
    public class PivotPathConditionDescription
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
        [JsonProperty("objectFilterID")]
        public string ObjectFilterId { get; set; }
        [JsonProperty("objectFilterShortName")]
        public string ObjectFilterShortName { get; set; }
    }
}
