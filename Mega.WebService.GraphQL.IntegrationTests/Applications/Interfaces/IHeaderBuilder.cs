using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces
{
    interface IHeaderBuilder
    {
        Task FillHeadersAsync(SessionDatas parameters, HttpRequestHeaders headers);
    }
}
