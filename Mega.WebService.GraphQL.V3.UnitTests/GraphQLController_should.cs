using System;
using FluentAssertions;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Moq;
using System.Web.Http.Results;
using Xunit;
using static Mega.WebService.GraphQL.V3.UnitTests.Assertions.CallMacroArgumentsMatchers<object>;
using static Mega.WebService.GraphQL.V3.UnitTests.MacroResultBuilder;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class GraphQLController_should
    {

        [Fact]
        public async void Export_schema()
        {
            var mockMacro = new Mock<IMacroCaller>();
            var controller = new TestableGraphQlController()
            {
                MacroCaller = mockMacro.Object                
            };
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
            var mockMacro = new Mock<IMacroCaller>();
            var controller = new TestableGraphQlController()
            {
                MacroCaller = mockMacro.Object
            };
            mockMacro.Setup(m => m.CallMacro("AAC8AB1E5D25678E", HasJsonArg(a => a.Request.Path.EndsWith("/api/schema/v3/ITPM"))))
                .Returns(Ok(@"{""schema"": ""abcdefg""}"));

            var actionResult = controller.ExportSchema("ITPM", "v3");

            var content = ((ResponseMessageResult)actionResult).Response.Content;
            content.Headers.ContentType.ToString().Should().Be("text/plain");
            content.Headers.ContentDisposition.FileName.Should().Be("ITPM.graphql");
            (await content.ReadAsStringAsync()).Should().Be("abcdefg");
        }
    }

    class TestableGraphQlController : GraphQlController
    {
        internal IMacroCaller MacroCaller;

        protected override WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            return MacroCaller.CallMacro(macroId, data);
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
