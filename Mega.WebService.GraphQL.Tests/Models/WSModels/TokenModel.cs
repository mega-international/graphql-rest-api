using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Tests.Models.WSModels
{
    public class TokenModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
