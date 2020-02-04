using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Tests.Models.SessionDatas
{
    public class SessionData
    {
        [JsonProperty("id")]
        public readonly string Id;

        [JsonProperty("name")]
        public readonly string Name;
        public SessionData(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
