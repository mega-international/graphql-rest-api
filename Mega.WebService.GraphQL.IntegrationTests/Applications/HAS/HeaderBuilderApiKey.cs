using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.HAS
{
    class HeaderBuilderApiKey : IHeaderBuilder
    {
        public Task FillHeadersAsync(SessionDatas sessionDatas, HttpRequestHeaders headers)
        {
            headers.Add("X-Api-Key", sessionDatas.ApiKey);
            return Task.CompletedTask;
        }
    }
}
