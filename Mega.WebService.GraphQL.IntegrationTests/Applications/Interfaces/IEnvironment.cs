namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces
{
    internal interface IEnvironment
    {
        string Id { get; }
        string Path { get; }
        IRepository GetRepositoryByName(string repositoryName);
        void SetCurrentAdmin(string adminName, string password);
        IRepository CreateRepository(string name, string options);
    }
}
