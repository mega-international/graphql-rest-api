using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class AttachmentArguments
    {
        [JsonProperty("idUploadSession")]
        public string IdUploadSession { get; set; }
        [JsonProperty("updateMode")]
        public UpdateMode UpdateMode { get; set; }
    }

    public enum UpdateMode
    {
        Replace = 0, // Overwrite latest version
        New = 1      // Create a new version
    }
}
