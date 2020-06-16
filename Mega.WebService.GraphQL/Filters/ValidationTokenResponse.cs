using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Filters
{
    public class ValidationTokenResponse
    {
        [JsonProperty("exp")]
        public string ExpireAt { get; set; }

        public string Message { get; set; }
    }
}
