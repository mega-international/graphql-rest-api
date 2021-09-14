using Newtonsoft.Json;
using System;

namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public class HASSessionInfos : ISessionInfos
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        public ISessionInfos CloneWithProfile(string profileId)
        {
            throw new NotImplementedException("Could not use another profile for HASMode, please use old mode");
        }
    }
}
