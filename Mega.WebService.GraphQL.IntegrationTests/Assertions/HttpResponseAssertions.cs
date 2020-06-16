using FluentAssertions;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Mega.WebService.GraphQL.IntegrationTests.Assertions
{   
    class HttpResponseAssertions : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseAssertions>
    {
        public HttpResponseAssertions(HttpResponseMessage instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HttpResponseAssertions";

        public async Task<AndConstraint<HttpResponseAssertions>> BeOkUploadAsync(BusinessDocument expectedDocument)
        {
            var myString = await Subject.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<BusinessDocumentUploadResponse>(myString);
            parsedResponse.documentId.Should().Be(expectedDocument.Id);
            parsedResponse.success.Should().Be(true);
            return new AndConstraint<HttpResponseAssertions>(this);
        }

        public async Task<AndConstraint<HttpResponseAssertions>> BeDiagramAsync()
        {
            var svg = await Subject.Content.ReadAsStringAsync();
            var xml = XDocument.Parse(svg);
            xml.Should().HaveRoot(XName.Get("svg", "http://www.w3.org/2000/svg"));
            ShouldHaveEmbeddedBitmaps(xml);
            return new AndConstraint<HttpResponseAssertions>(this);
        }

        private static void ShouldHaveEmbeddedBitmaps(XDocument xml)
        {
            var images = xml.Descendants(XName.Get("image", "http://www.w3.org/2000/svg"));
            images.Should().HaveCountGreaterThan(0);
            foreach (var image in images)
            {
                var imageContent = image.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink")).Value;
                imageContent.Should().Match("data:image/*;base64*");
            }
        }
    }

    static class HttpResponseExtension
    {
        public static HttpResponseAssertions Should(this HttpResponseMessage instance)
        {
            return new HttpResponseAssertions(instance);
        }
    }
}
