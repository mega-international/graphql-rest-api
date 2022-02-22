using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class UploadDeleteSessionResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
