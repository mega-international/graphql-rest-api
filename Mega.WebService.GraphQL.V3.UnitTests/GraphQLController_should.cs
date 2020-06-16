using FluentAssertions;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using Moq;
using System.Web.Http.Results;
using Xunit;
using static Mega.WebService.GraphQL.V3.UnitTests.Assertions.CallMacroArgumentsMatchers<object>;
using static Mega.WebService.GraphQL.V3.UnitTests.MacroResultBuilder;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using Hopex.Common;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class GraphQLController_should
    {
        Mock<IMacroCall> mockMacro = new Mock<IMacroCall>();
        TestableGraphQlController controller;

        public GraphQLController_should()
        {
            controller = new TestableGraphQlController(mockMacro.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "https://host/api/thing2")
            };
        }

        [Fact]
        public async void Export_schema()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path.EndsWith("/api/schema/ITPM"))))
                .Returns(Ok(@"{""schema"": ""abcdefg""}"));

            var actionResult = controller.ExportSchema("ITPM");

            var content = ((ResponseMessageResult)actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be("text/plain");
            content.Headers.ContentDisposition.FileName.Should().Be("ITPM.graphql");
            (await content.ReadAsStringAsync()).Should().Be("abcdefg");
        }

        [Fact]
        public async void Export_schema_in_a_specific_version()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path.EndsWith("/api/schema/v3/ITPM"))))
                .Returns(Ok(@"{""schema"": ""abcdefg""}"));

            var actionResult = controller.ExportSchema("ITPM", "v3");

            var content = ((ResponseMessageResult)actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be("text/plain");
            content.Headers.ContentDisposition.FileName.Should().Be("ITPM.graphql");
            (await content.ReadAsStringAsync()).Should().Be("abcdefg");
        }


        [Fact]
        public void Execute_sync_query()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", It.IsAny<string>()))
                .Returns(Ok(@"{""HttpStatusCode"":200, ""Result"":""{\""ok\"":true}""}"));

           var result = controller.Execute("ITPM", new InputArguments());

           result.Should().BeJson(@"{""ok"":true}");
        }

        [Fact]
        public void Execute_async_query()
        {
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", It.IsAny<string>()))
                .Returns(Ok(@"{""HttpStatusCode"":200, ""Result"":""{\""ok\"":true}""}"));

            var result = controller.AsyncExecute("ITPM", new InputArguments());

            var content = result as ResponseMessageResult;
            var taskId = content.Response.Headers.GetValues("x-hopex-task").First();
            controller.Request.Headers.Add("x-hopex-task", taskId);

            result = controller.AsyncExecute("ITPM", new InputArguments());

            result.Should().BeJson(@"{""ok"":true}");
        }
    }

    class TestableGraphQlController : GraphQlController
    {
        private FakeMacroCaller _macroCaller;

        internal TestableGraphQlController(IMacroCall macroCaller)
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

        protected override bool TryParseHopexContext(ref HopexContext hopexContext)
        {
            hopexContext = new HopexContext
            {
                EnvironmentId = "FakeEnvId"
            };
            return true;
        }
    }
}
