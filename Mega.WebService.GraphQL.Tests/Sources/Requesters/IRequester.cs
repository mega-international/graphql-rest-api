using GraphQL.Common.Request;
using GraphQL.Common.Response;
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public interface IRequester
    {
        void SetConfig(ISessionInfos sessionInfos);
        Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode);
        Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode, CancellationToken cancellationToken);
    }
}
