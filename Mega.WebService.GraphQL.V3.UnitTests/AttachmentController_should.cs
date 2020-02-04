using FluentAssertions;
using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using Moq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http.Results;
using Xunit;
using static Mega.WebService.GraphQL.V3.UnitTests.Assertions.CallMacroArgumentsMatchers<Hopex.Common.JsonMessages.AttachmentArguments>;
using static Mega.WebService.GraphQL.V3.UnitTests.MacroResultBuilder;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class AttachmentController_should
    {
        const string DOCUMENT_ID = "ABCDEF";
        readonly Mock<IMacroCaller> mockMacro = new Mock<IMacroCaller>();
        readonly TestableAttachmentController controller;

        public AttachmentController_should()
        {
            controller = new TestableAttachmentController
            {
                MacroCaller = mockMacro.Object,
                Request = new HttpRequestMessage()
            };
        }

        [Theory]
        [InlineData(UpdateMode.Replace, "replace")]
        [InlineData(UpdateMode.New, "New")]
        public async void Upload_a_file(UpdateMode updateMode, string updateHeaderValue)
        {
            mockMacro.Setup(m => m.CallMacro("CD31CDAB4F865BEB", It.IsAny<string>()))
                .Returns(new WebServiceResult{ ErrorType = "None" }).Verifiable();
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path.EndsWith("uploadfile") && a.GetUserData().UpdateMode == updateMode)))
                .Returns(Ok($@"{{""documentId"": ""{DOCUMENT_ID}"", ""success"": true}}"));
            controller.Request.Headers.Add("X-Hopex-Filename", "foo.txt");
            controller.Request.Headers.Add("X-Hopex-DocumentVersion", updateHeaderValue);

            var actionResult = await controller.UploadFile(DOCUMENT_ID);

            mockMacro.Verify();
            actionResult.Should().BeJson($@"{{""documentId"": ""{DOCUMENT_ID}"", ""success"": true}}");            
        }

        [Fact]
        public async void Warn_if_UpdateMode_not_recognized()
        {
            controller.Request.Headers.Add("X-Hopex-Filename", "foo.txt");
            controller.Request.Headers.Add("X-Hopex-DocumentVersion", "NotValid");

            var actionResult = await controller.UploadFile(DOCUMENT_ID);

            actionResult.Should().BeBadRequest("*X-Hopex-DocumentVersion*Replace*New*");
        }


        [Fact]
        public async void Warn_if_missing_UpdateMode()
        {
            controller.Request.Headers.Add("X-Hopex-Filename", "foo.txt");

            var actionResult = await controller.UploadFile(DOCUMENT_ID);

            actionResult.Should().BeBadRequest("*X-Hopex-DocumentVersion*");
        }

        [Fact]
        public async void Warn_if_missing_filename()
        {
            controller.Request.Headers.Add("X-Hopex-DocumentVersion", "Overwrite");

            var actionResult = await controller.UploadFile(DOCUMENT_ID);

            actionResult.Should().BeBadRequest("*X-Hopex-Filename*");
        }

        [Fact]
        public async void Download_a_file()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path.EndsWith("downloadfile"))))
                .Returns(Ok($@"{{""documentId"": ""{DOCUMENT_ID}"", ""contentType"": ""text/plain"", ""content"":""YWJjZGVmZw=="", ""fileName"":""foo.txt""}}"));

            var actionResult = controller.DownloadFile(DOCUMENT_ID);

            var content = ((ResponseMessageResult) actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be("text/plain");
            content.Headers.ContentDisposition.FileName.Should().Be("foo.txt");
            (await content.ReadAsStringAsync()).Should().Be("abcdefg");            
        }
    }

    class TestableAttachmentController : AttachmentController
    {

        internal IMacroCaller MacroCaller;
                
        protected override WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            return MacroCaller.CallMacro(macroId, data);
        }

        protected override Stream GetRequestBufferlessStream()
        {
            return new MemoryStream(Encoding.ASCII.GetBytes("1234"));
        }
    }
}
