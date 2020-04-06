using FluentAssertions;
using FluentAssertions.Primitives;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.Assertions
{
    class HttpContentAssertions : ReferenceTypeAssertions<HttpContent, HttpContentAssertions>
    {
        public HttpContentAssertions(HttpContent instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HttpContentAssertions";

        public async Task<AndConstraint<HttpContentAssertions>> BeFileAsync(string filename, string content)
        {
            (await Subject.ReadAsStringAsync()).Should().Be(content);
            var contentDisposition = Subject.Headers.ContentDisposition;
            contentDisposition.FileName.Should().Be($"\"{filename}\"");
            contentDisposition.DispositionType.Should().Be("attachment");
            return new AndConstraint<HttpContentAssertions>(this);
        }
    }


    static class HttpContentExtension
    {
        public static HttpContentAssertions Should(this HttpContent instance)
        {
            return new HttpContentAssertions(instance);
        }
    }
}
