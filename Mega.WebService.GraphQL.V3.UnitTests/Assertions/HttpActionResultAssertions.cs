using FluentAssertions;
using FluentAssertions.Json;
using FluentAssertions.Primitives;
using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

namespace Mega.WebService.GraphQL.V3.UnitTests.Assertions
{
    static class HttpActionResultExtension
    {
        public static HttpActionResultAssertions Should(this IHttpActionResult instance)
        {
            return new HttpActionResultAssertions(instance);
        }
    }

    class HttpActionResultAssertions : ReferenceTypeAssertions<IHttpActionResult, HttpActionResultAssertions>
    {

        public HttpActionResultAssertions(IHttpActionResult instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HopexResponse";

        public AndConstraint<HttpActionResultAssertions> BeJson(string expected)
        {
            var contentResult = Subject as OkNegotiatedContentResult<object>;
            JToken.Parse(contentResult.Content.ToString()).Should().BeEquivalentTo(JToken.Parse(expected));
            return new AndConstraint<HttpActionResultAssertions>(this);
        }

        public AndConstraint<HttpActionResultAssertions> BeBadRequest(string wildcardPattern)
        {
            var badRequestResult = Subject as BadRequestErrorMessageResult;
            badRequestResult.Message.Should().Match(wildcardPattern);
            return new AndConstraint<HttpActionResultAssertions>(this);
        }

        public AndConstraint<HttpActionResultAssertions> BeError(HttpStatusCode statusCode, string expected)
        {
            var contentResult = Subject as NegotiatedContentResult<ErrorContent>;
            contentResult.StatusCode.Should().Be(statusCode);
            contentResult.Content.Error.Should().Be(expected);
            return new AndConstraint<HttpActionResultAssertions>(this);
        }
    }
}
