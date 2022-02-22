using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class UploadCreateSessionResult
    {
        [JsonProperty("idUploadSession")]
        public string IdUploadSession { get; set; }
        [JsonProperty("bMaxUploadsReached")]
        public bool MaxUploadsReached { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
