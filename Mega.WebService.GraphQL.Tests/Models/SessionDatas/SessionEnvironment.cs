using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.SessionDatas
{
    public class SessionEnvironment : SessionData
    {
        [JsonProperty("repositories")]
        public readonly List<SessionRepository> Repositories = new List<SessionRepository>();

        public SessionEnvironment(string id, string name) : base(id, name) {}

        public SessionEnvironment(SessionData sessionData) :
            this(sessionData.Id, sessionData.Name) {}
    }
}
