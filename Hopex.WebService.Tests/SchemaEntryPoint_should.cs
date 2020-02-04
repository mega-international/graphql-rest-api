using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Assertions;
using Xunit;
using Hopex.WebService.Tests.Mocks;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Mocks;

namespace Hopex.WebService.Tests
{
    public class SchemaEntryPoint_should
    {
        [Fact]
        public async void Export_schema()
        {
            var entryPoint = new TestableSchemaEntryPoint("ITPM");

            var result = await entryPoint.Execute(null);

            result.Should().MatchJson("schema", "*type Application {*");
        }

        [Fact]
        public async void Export_schema_of_a_specific_version()
        {
            var entryPoint = new TestableSchemaEntryPoint("v3/ITPM");

            var result = await entryPoint.Execute(null);

            result.Should().MatchJson("schema", "*type Application {*");
        }
    }

    public class TestableSchemaEntryPoint : SchemaEntryPoint
    {
        public TestableSchemaEntryPoint(string schema)
            :base(new TestableSchemaManagerProvider())
        {
            var _request = new FakeSchemaRequest(schema);
            (this as IHopexWebService).SetHopexContext(new MockMegaRoot(), _request, new Logger());
        }

        protected override IMegaRoot GetRoot()
        {
            return (IMegaRoot)HopexContext.NativeRoot;
        }
    }

    internal class FakeSchemaRequest : BaseMockRequest
    {
        private string _path;

        public FakeSchemaRequest(string pathEnd)
        {
            _path = $"/api/schema/{pathEnd}";
        }

        public override string Path => _path;
    }
}
