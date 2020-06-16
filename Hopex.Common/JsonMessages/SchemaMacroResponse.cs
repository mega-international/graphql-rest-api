using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class SchemaMacroResponse
    {
        [JsonProperty("schema")]
        public string Schema { get; set; }
    }
}
