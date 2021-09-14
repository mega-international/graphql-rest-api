namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public class SessionInfos : ISessionInfos
    {
        public string Environment { get; set; }
        public string Repository { get; set; }
        public string Profile { get; set; }

        public ISessionInfos CloneWithProfile(string profileId)
        {
            return new SessionInfos
            {
                Environment = Environment,
                Repository = Repository,
                Profile = profileId
            };
        }
    }
}
