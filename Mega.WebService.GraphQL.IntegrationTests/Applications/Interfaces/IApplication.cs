namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces
{
    internal interface IApplication
    {
        IEnvironment GetEnvironment();
        IHeaderBuilder InstanciateBuilder();
    }
}
