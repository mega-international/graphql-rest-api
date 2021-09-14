namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public interface ISessionInfos
    {
        ISessionInfos CloneWithProfile(string profileId);
    }
}
