using System.Linq;
using Hopex.ApplicationServer.WebServices;
using System.Threading.Tasks;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Mocks;
using Xunit;
using FluentAssertions;
using Hopex.WebService.Tests.Assertions;
using Hopex.Model.Mocks;
using Moq;

namespace Hopex.WebService.Tests
{
    public class EntryPoint_should
    {
        readonly MockEntryPoint ws = new MockEntryPoint(maxCollectionSize:1);

        [Theory]
        [InlineData("query {application(filter:{id: \"bGMnovy8NP27\"}) {  id}}", "SELECT ~MrUiM9B5iyM0[Application] WHERE ~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\"")]
        [InlineData(
            "query{application(filter:{ id: \"bGMnovy8NP27\", modificationDate: \"2019-01-01\"}) {id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE (~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\"  AND ~610000000P00[ModificationDate] = ~G2H9KK2qI100{D}[2019/01/01] )"
        )]
        [InlineData("query {application(filter:{id_not: \"bGMnovy8NP27\"}) {  id}}", "SELECT ~MrUiM9B5iyM0[Application] WHERE ~310000000D00[Absolute Identifier] Not= \"bGMnovy8NP27\"")]
        [InlineData(
            "query {application(filter:{ or: {id: \"bGMnovy8NP27\" name_contains: \"Virtual\"}}) {  id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE (~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\"  or ~Z20000000D60[Name] Like \"#Virtual#\" )"
        )]
        [InlineData(
            "query{application{id name applicationOwner_PersonSystem(filter:{name:\"Anne\"}){id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application]",
            "SELECT ~T20000000s10[PersonSystem] WHERE ~210000000900[Name] = \"Anne\"  AND ~H20000008a80[Assignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[AbsoluteIdentifier] = \"~WzF2lb0yGb2U\" AND ~hCr81RIpEvMH[AssignedObject]:~MrUiM9B5iyM0[Application].~310000000D00[AbsoluteIdentifier] = \"application-100\" )"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_some: {name:\"nom du process\"}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess]:~pj)grmQ9pG90[BusinessProcess].(~Z20000000D60[Name] = \"nom du process\" )",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].~310000000D00[AbsoluteIdentifier] = \"application-100\""
        )]
        [InlineData(
            "query { application(filter:{businessCapability_some:{name: \"nom de la capa\"}}) {	id  name  businessCapability { id  name }}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~JdRdxwf6EX5P[OwnedBusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].~od9L6Ys9Bzf1[FulfilledBusinessCapability]:~IcfsZhjW9T90[BusinessCapability].(~Z20000000D60[Name] = \"nom de la capa\" )",
            "SELECT ~IcfsZhjW9T90[BusinessCapability] WHERE ~nd9L6Ys9Bvf1[BusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].~IdRdxwf6ET5P[FulfillingEnterpriseAgent]:~MrUiM9B5iyM0[Application].~310000000D00[AbsoluteIdentifier] = \"application-100\""
        )]
        [InlineData(
            "query { application{ id name businessProcess(filter:{name: \"Financial\"}) {id  name } }}",
            "SELECT ~MrUiM9B5iyM0[Application]",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~Z20000000D60[Name] = \"Financial\"  AND ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].~310000000D00[AbsoluteIdentifier] = \"application-100\""
        )]
        [InlineData(
            "query { application{ id name businessCapability(filter:{name: \"Financial\"}) {id name} }}",
            "SELECT ~MrUiM9B5iyM0[Application]",
            "SELECT ~IcfsZhjW9T90[BusinessCapability] WHERE ~Z20000000D60[Name] = \"Financial\"  AND ~nd9L6Ys9Bvf1[BusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].~IdRdxwf6ET5P[FulfillingEnterpriseAgent]:~MrUiM9B5iyM0[Application].~310000000D00[AbsoluteIdentifier] = \"application-100\""
        )]
        public async Task Query_with_filter(string query, params string[] expectedERQLs)
        {
            var resp = await ExecuteQueryAsync(@query);
            var erqls = (ws.GetMegaRoot() as ISupportsDiagnostics).GeneratedERQLs;
            erqls.Should().BeEquivalentTo(expectedERQLs);
        }

        [Fact]
        public async Task Query_an_object()
        {
            var resp = await ExecuteQueryAsync(@"{application {name}}");

            var collection = await ws.DataModel.GetCollectionAsync("Application");
            resp.Should().ContainsGraphQLCount("data.application", collection.Count());
        }

        [Fact]
        public async Task Query_a_relationship()
        {
            var resp = await ExecuteQueryAsync(@"{application {name businessCapability {name id } } }");

            resp.Should().MatchGraphQL("data.application[0].businessCapability[0].name", "*");
        }

        [Fact]
        public async Task Return_upload_and_download_links_for_business_documents()
        {
            var resp = await ExecuteQueryAsync(@"{businessDocument {id downloadLink uploadLink}}");

            resp.Should().MatchGraphQL("data.businessDocument[0].downloadLink", "*/file");
            resp.Should().MatchGraphQL("data.businessDocument[0].uploadLink", "*/file");
        }

        [Fact]
        public async Task Create_an_application()
        {
            var resp = await ExecuteQueryAsync(@"mutation { createApplication (application : { name:""test""} ) {id name }}");

            Assert.Equal(200, resp.StatusCode);
            var collection = await ws.DataModel.GetCollectionAsync("Application");
            var created = collection.FirstOrDefault(ap => ap.GetValue<string>("Name") == "test");
            Assert.NotNull(created);
        }

        [Fact]
        public async Task Create_an_application_with_relationShips()
        {
            var resp = await ExecuteQueryAsync(@"mutation { 
                createApplication ( application : {
                    name:""test""
                    softwareTechnology_UsedTechnology: {
                        action: add list: [{id:""businesscapability-0""}]
                    } } )
                {id name }}");

            Assert.Equal(200, resp.StatusCode);
            var collection = await ws.DataModel.GetCollectionAsync("Application");
            var created = collection.FirstOrDefault(ap => ap.GetValue<string>("Name") == "test");
            Assert.NotNull(created);
        }

        [Fact]
        public async Task Enforce_maxLength()
        {
            var nameTooLong = new string('x', 1025);
            
            var resp = await ExecuteQueryAsync($@"mutation {{ createApplication (application : {{ name:""{nameTooLong}""}} ) {{id name }}}}");

            resp.Should().ContainsGraphQL("errors[0].message", $"Value {nameTooLong} for Name exceeds maximum length of 1024");
            var collection = await ws.DataModel.GetCollectionAsync("Application");
            collection.Should().NotContain(a => a.GetValue<string>("Name", null).Contains("xxxxx"));
        }

        private async Task<HopexResponse> ExecuteQueryAsync(string query)
        {
            return await ws.Execute(new InputArguments
            {
                Query = query
            });
        }
    }
}
