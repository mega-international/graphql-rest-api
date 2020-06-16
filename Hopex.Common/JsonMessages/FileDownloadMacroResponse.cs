using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class FileDownloadMacroResponse
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
