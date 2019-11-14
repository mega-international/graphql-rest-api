using System.Linq;
using Hopex.ApplicationServer.WebServices;
using System.Threading.Tasks;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Mocks;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class WebServiceTests
    {
        [Fact]
        public async Task BasicQuery()
        {
            MockEntryPoint ws = new MockEntryPoint();
            InputArguments a = new InputArguments
            {
                query = @"{application {shortName @format(name:""external"")}}"
            };
            HopexResponse resp = await ws.Execute(a);
            Assert.Equal(200, resp.StatusCode);
        }

        [Fact]
        public async Task QueryWithRelationships()
        {
            MockEntryPoint ws = new MockEntryPoint();
            InputArguments a = new InputArguments
            {
                query = @"{application {shortName businessCapability {shortName id } }"
            };
            HopexResponse resp = await ws.Execute(a);
            Assert.Equal(200, resp.StatusCode);
        }

        [Fact]
        public async Task CreateApplication()
        {
            MockEntryPoint ws = new MockEntryPoint();
            InputArguments a = new InputArguments
            {
                query = @"mutation { createApplication (application : { shortName:""test""} ) {id shortName }}"
            };
            HopexResponse resp = await ws.Execute(a);
            Assert.Equal(200, resp.StatusCode);

            var collection = await ws.DataModel.GetCollectionAsync("Application");
            var created = collection.FirstOrDefault(ap => ap.GetValue<string>("ShortName") == "test");
            Assert.NotNull(created);
        }

        [Fact]
        public async Task CreateApplicationWithRelationShips()
        {
            MockEntryPoint ws = new MockEntryPoint();
            var elemId = "businesscapability-1";
            InputArguments a = new InputArguments
            {
                query = "mutation { createApplication (application : { shortName:\"test\" usedTechnology: {action: add list: [\"" + elemId + "\"]}} ) {id shortName }}"
            };

            HopexResponse resp = await ws.Execute(a);
            Assert.Equal(200, resp.StatusCode);

            var collection = await ws.DataModel.GetCollectionAsync("Application");
            var created = collection.FirstOrDefault(ap => ap.GetValue<string>("ShortName") == "test");
            Assert.NotNull(created);
        }
    }
}
