using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Filters
{
    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        private AuthenticationHeaderValue Challenge { get; }

        private IHttpActionResult InnerResult { get; }

        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await InnerResult.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.All(h => h.Scheme != Challenge.Scheme))
                response.Headers.WwwAuthenticate.Add(Challenge);

            return response;
        }

    }
}
