using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Tests.Models.SessionDatas
{
    public class SessionRepository : SessionData
    {
        [JsonIgnore]
        public readonly SessionEnvironment Environment;
        public SessionRepository(string id, string name, SessionEnvironment environment) : base(id, name)
        {
            Environment = environment;
            environment.Repositories.Add(this);
        }

        public SessionRepository(SessionData sessionData, SessionEnvironment environment) :
            this(sessionData.Id, sessionData.Name, environment) { }
    }
}
