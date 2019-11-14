using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Models
{
  public class UasToken
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
  }
}
