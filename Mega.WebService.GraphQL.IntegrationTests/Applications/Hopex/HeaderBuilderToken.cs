using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Hopex
{
    class HeaderBuilderToken : IHeaderBuilder
    {
        private UasToken _uasToken = UasToken.NO_TOKEN;
        public async Task FillHeadersAsync(SessionDatas sessionDatas, HttpRequestHeaders headers)
        {
            if (_uasToken.Expired())
            {
                _uasToken = await UasToken.CreateAsync(sessionDatas);
            }
            headers.Authorization = AuthenticationHeaderValue.Parse($"Bearer {_uasToken.AccessToken}");
            headers.Add("X-Hopex-Context", $"{{\"EnvironmentId\":\"{sessionDatas.EnvironmentId}\",\"RepositoryId\":\"{sessionDatas.RepositoryId}\",\"ProfileId\":\"{sessionDatas.ProfileId}\"}}");
        }
    }
}
