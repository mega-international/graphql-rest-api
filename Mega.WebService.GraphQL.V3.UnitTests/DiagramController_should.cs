using FluentAssertions;
using FluentAssertions.Json;
using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Xunit;
using Xunit.Abstractions;
using static Mega.WebService.GraphQL.V3.UnitTests.Assertions.CallMacroArgumentsMatchers<Hopex.Common.JsonMessages.DiagramExportArguments>;
using static Mega.WebService.GraphQL.V3.UnitTests.MacroResultBuilder;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public partial class DiagramController_should
    {

        readonly Mock<IMacroCall> mockMacro = new Mock<IMacroCall>();
        readonly TestableDiagramController controller;

        public DiagramController_should()
        {
            controller = new TestableDiagramController(mockMacro.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        public static IEnumerable<object[]> HeadersData =>
            new[]
            {
                new object[] { "image/png", ExpectedFormat.Png},
                new object[] { "image/jpeg", ExpectedFormat.Jpeg},
                new object[] { "image/jpeg, image/png", ExpectedFormat.Png},
                new object[] { "image/png;q=0.9, image/jpeg", ExpectedFormat.Jpeg},
                new object[] { "image/gif, */*;q=0.1", ExpectedFormat.Png},
                new object[] { "image/*", ExpectedFormat.Png},
                new object[] { null, ExpectedFormat.Png}
            };
        [Theory]
        [MemberData(nameof(HeadersData))]
        public async void Return_a_diagram_bitmap(string acceptHeader, ExpectedFormat expected)
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(
               a => a.Request.Path == "/api/diagram/BGZ4uPrgG(Up/image" && a.GetUserData().Format == expected.MegaFormat)))
                .Returns(Ok($@"{{""contentType"": ""{expected.MimeType}"", ""content"":""AQIDBA=="", ""fileName"":""Library diagram.{expected.Extension}""}}"));
            controller.Request.Headers.Accept.ParseAdd(acceptHeader);

            var actionResult = controller.GetImage("BGZ4uPrgG(Up");

            var content = ((ResponseMessageResult)actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be(expected.MimeType);
            content.Headers.ContentDisposition.FileName.Trim('"').Should().Be($"Library diagram.{expected.Extension}");
            var actualBytes = await content.ReadAsByteArrayAsync();
            actualBytes.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
        }

        [Fact]
        public async void Return_a_diagram_svg()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(
               a => a.Request.Path == "/api/diagram/BGZ4uPrgG(Up/image" && a.GetUserData().Format == ImageFormat.Svg)))
                .Returns(Ok($@"{{""contentType"": ""image/svg+xml"", ""content"":""PHN2Zz5hYmNkPC9zdmc+"", ""fileName"":""Library diagram.svg""}}"));
            controller.Request.Headers.Accept.ParseAdd("image/svg+xml");

            var actionResult = controller.GetImage("BGZ4uPrgG(Up");

            var content = ((ResponseMessageResult)actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be("image/svg+xml");
            content.Headers.ContentDisposition.FileName.Trim('"').Should().Be($"Library diagram.svg");
            var actual = await content.ReadAsStringAsync();
            actual.Should().Be("<svg>abcd</svg>");
        }

        [Fact]
        public void Reject_unknown_format()
        {
            controller.Request.Headers.Accept.ParseAdd("image/gif");

            var actionResult = controller.GetImage("BGZ4uPrgG(Up");

            ((StatusCodeResult)actionResult).StatusCode.Should().Be(406);
        }

        [Theory]
        [InlineData("100", 1)]
        [InlineData("1", 255)]
        [InlineData("50", 129)]
        public void Change_jpeg_quality(string quality, int expectedMegaQuality)
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.GetUserData().Quality == expectedMegaQuality)))
                .Returns(Ok($@"{{""contentType"": ""image/jpeg"", ""content"":""AQIDBA=="", ""fileName"":""Library diagram.jpg""}}"))
                .Verifiable();
            controller.Request.Headers.Accept.ParseAdd("image/jpeg");
            controller.Request.Headers.Add("X-Hopex-JpegQuality", quality);

            var actionResult = controller.GetImage("BGZ4uPrgG(Up");

            mockMacro.Verify();
        }

        [Theory]
        [InlineData("800")]
        [InlineData("-1")]
        [InlineData("good")]
        public void Reject_bad_jpeg_quality(string badQuality)
        {
            controller.Request.Headers.Accept.ParseAdd("image/jpeg");
            controller.Request.Headers.Add("X-Hopex-JpegQuality", badQuality);

            var actionResult = controller.GetImage("BGZ4uPrgG(Up");

            actionResult.Should().BeBadRequest("*X-Hopex-JpegQuality*"); ;
        }

        [Fact]
        public async void Execute_async_diagram_query()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", It.IsAny<string>()))
                .Returns(Ok($@"{{""contentType"": ""image/svg+xml"", ""content"":""PHN2Zz5hYmNkPC9zdmc+"", ""fileName"":""Library diagram.svg""}}"));
            controller.Request.Headers.Accept.ParseAdd("image/svg+xml");

            var result = AsyncRequestHelper.PlayAsyncRequest(controller, () => controller.AsyncGetImage("BGZ4uPrgG(Up"));

            var content2 = ((ResponseMessageResult)result).Response.Content;
            content2.Headers.ContentType.ToString().Should().Be("image/svg+xml");
            content2.Headers.ContentDisposition.FileName.Trim('"').Should().Be($"Library diagram.svg");
            var actual = await content2.ReadAsStringAsync();
            actual.Should().Be("<svg>abcd</svg>");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class ExpectedFormat : IXunitSerializable
        {
            public ImageFormat MegaFormat { get; set; }
            public string MimeType { get; set; }
            public string Extension { get; set; }

            static internal ExpectedFormat Png = new ExpectedFormat { MegaFormat = ImageFormat.Png, MimeType = "image/png", Extension = "png" };
            static internal ExpectedFormat Jpeg = new ExpectedFormat { MegaFormat = ImageFormat.Jpeg, MimeType = "image/jpeg", Extension = "jpeg" };

            [ExcludeFromCodeCoverage]
            public void Deserialize(IXunitSerializationInfo info)
            {
                var format = JsonConvert.DeserializeObject<ExpectedFormat>(info.GetValue<string>("objValue"));
                Extension = format.Extension;
                MimeType = format.MimeType;
                MegaFormat = format.MegaFormat;
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("objValue", JsonConvert.SerializeObject(this));
            }

            public override string ToString() => Extension;
        }

        class TestableDiagramController : DiagramController
        {

            private FakeMacroCaller _macroCaller;

            internal TestableDiagramController(IMacroCall macroCaller)
            {
                _macroCaller = new FakeMacroCaller(macroCaller);
            }

            protected override WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
            {
                return _macroCaller.CallMacro(macroId, data, sessionMode, accessMode, closeSession);
            }

            protected override IHttpActionResult CallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
            {
                return ResponseMessage(_macroCaller.CallAsyncMacroExecute(macroId, data, sessionMode, accessMode, closeSession));
            }

            protected override IHttpActionResult CallAsyncMacroGetResult(string hopexTask, bool closeSession = false)
            {
                return BuildActionResultFrom(_macroCaller.CallAsyncMacroGetResult(hopexTask, closeSession));
            }
        }

    }
}
