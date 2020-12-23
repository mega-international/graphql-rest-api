using FluentAssertions;
using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using Moq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Xunit;

using static Mega.WebService.GraphQL.V3.UnitTests.Assertions.CallMacroArgumentsMatchers<Hopex.Common.JsonMessages.DatasetArguments>;
using static Mega.WebService.GraphQL.V3.UnitTests.MacroResultBuilder;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class DatasetController_should
    {
        DatasetController _controller;
        Mock<IMacroCall> _mockMacro = new Mock<IMacroCall>(MockBehavior.Strict);

        public DatasetController_should()
        {
            _controller = new TestableDatasetController(_mockMacro.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        [Fact]
        public void Export_a_dataset()
        {
            _mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path == "/api/dataset/7IST0Q78NzfG/content" && a.GetUserData().Regenerate == false)))
                .Returns(Ok(@"{""data"":[]}"))
                .Verifiable();

            var actionResult = _controller.GetContent("7IST0Q78NzfG");

            _mockMacro.Verify();
            actionResult.Should().BeJson(@"{""data"":[]}");
        }

        [Fact]
        public void Regenerate_a_dataset()
        {
            _controller.Request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            _mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path == "/api/dataset/7IST0Q78NzfG/content" && a.GetUserData().Regenerate == true)))
                .Returns(Ok(@"{""data"":[]}"))
                .Verifiable();

            var actionResult = _controller.GetContent("7IST0Q78NzfG");

            _mockMacro.Verify();
            actionResult.Should().BeJson(@"{""data"":[]}");
        }

        [Theory]
        [InlineData("Never", DatasetNullValues.Never)]
        [InlineData("FirstLine", DatasetNullValues.FirstLine)]
        [InlineData("Always", DatasetNullValues.Always)]
        [InlineData(null, DatasetNullValues.FirstLine)]
        public void Control_null_values_output(string headerValue, DatasetNullValues expected)
        {
            _controller.Request.Headers.Add("X-Hopex-NullValues", headerValue);
            _mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.GetUserData().NullValues == expected)))
                .Returns(Ok(@"{""data"":[]}"))
                .Verifiable();

            var actionResult = _controller.GetContent("7IST0Q78NzfG");

            _mockMacro.Verify();
            actionResult.Should().BeJson(@"{""data"":[]}");
        }

        [Fact]
        public void Execute_async_dataset_query()
        {
            _mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", It.IsAny<string>()))
                .Returns(Ok(@"{""data"":[]}"));

            var actionResult = AsyncRequestHelper.PlayAsyncRequest(_controller, () => _controller.AsyncGetContent("7IST0Q78NzfG"));

            actionResult.Should().BeJson(@"{""data"":[]}");
        }

        [Fact]
        public void Return_error_if_dataset_is_invalid()
        {
            _mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", It.IsAny<string>()))
               .Returns(MacroError(HttpStatusCode.BadRequest, "Invalid dataset"));

            var actionResult = _controller.GetContent("7IST0Q78NzfG");

            actionResult.Should().BeError(HttpStatusCode.BadRequest, "Invalid dataset");
        }

        class TestableDatasetController : DatasetController
        {
            private FakeMacroCaller _macroCaller;

            internal TestableDatasetController(IMacroCall macroCaller)
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
