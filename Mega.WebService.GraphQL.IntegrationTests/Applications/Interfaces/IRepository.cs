namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces
{
    internal interface IRepository
    {
        string Id { get; }
        int Import(string ImportFileName, string RejectFileName, string Options);
        SessionDatas CreateSessionDatas(UserInfos userInfos, string profileId);
    }
}
