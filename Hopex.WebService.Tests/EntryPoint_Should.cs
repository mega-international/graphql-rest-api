using FluentAssertions;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common;
using Hopex.Model.Abstractions;
using Hopex.Model.Mocks;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;

using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Hopex.WebService.Tests
{
    public class EntryPoint_should
    {
        MockEntryPoint ws;

        [Theory]
        [InlineData(
            "query {application(filter:{id: \"bGMnovy8NP27\"}) {  id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\""
        )]
        //[InlineData(
        //    "query{application(filter:{ id: \"bGMnovy8NP27\", modificationDate: \"2019-01-01\"}) {id}}",
        //    "SELECT ~MrUiM9B5iyM0[Application] WHERE (~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\"  AND ~610000000P00[ModificationDate] = ~G2H9KK2qI100{D}[2019/01/01] )"
        //)]
        [InlineData(
            "query {application(filter:{id_not: \"bGMnovy8NP27\"}) {  id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~310000000D00[Absolute Identifier] Not= \"bGMnovy8NP27\""
        )]
        [InlineData(
            "query {application(filter:{ or: [{id: \"bGMnovy8NP27\" name_contains: \"Virtual\"}]}) {  id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE (~Z20000000D60[Name] Like \"#Virtual#\" OR ~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\")"
        )]
        [InlineData(
            "query{application{id name applicationOwner_PersonSystem(filter:{name:\"Anne\"}){id name}}}",
            "SELECT ~T20000000s10[PersonSystem] WHERE ~210000000900[Name] = \"Anne\" AND ~H20000008a80[Assignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~WzF2lb0yGb2U\" AND ~hCr81RIpEvMH[AssignedObject]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\"))"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_some: {name:\"nom du process\"}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess]:~pj)grmQ9pG90[BusinessProcess].(~Z20000000D60[Name] = \"nom du process\")"
        )]
        [InlineData(
            "query { application(filter:{businessCapability_some:{name: \"nom de la capa\"}}) {	id  name  businessCapability { id  name }}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~JdRdxwf6EX5P[OwnedBusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].(~od9L6Ys9Bzf1[FulfilledBusinessCapability]:~IcfsZhjW9T90[BusinessCapability].(~Z20000000D60[Name] = \"nom de la capa\"))"
        )]
        [InlineData(
            "query { application{ id name businessProcess(filter:{name: \"Financial\"}) {order id  name } }}",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~Z20000000D60[Name] = \"Financial\" AND ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\")"
        )]
        [InlineData(
            "query { application{ id name businessCapability(filter:{name: \"Financial\"}) {id name order} }}",
            "SELECT ~IcfsZhjW9T90[BusinessCapability] WHERE ~Z20000000D60[Name] = \"Financial\" AND ~nd9L6Ys9Bvf1[BusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].(~IdRdxwf6ET5P[FulfillingEnterpriseAgent]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\"))"
        )]
        [InlineData(
            "query { application(filter:{id: \"bGMnovy8NP27\"}) { businessProcess(filter:{linkComment:\"comment\"}) { linkComment } } }",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~310000000D00[Absolute Identifier] = \"bGMnovy8NP27\"",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].~C3cm9FyluS20[LinkComment] = \"comment\" AND ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\")"
        )]
        [InlineData(
            "query { application{ id name businessProcess(filter:{costContributionKeyBusinessProcess: 10}) {id name costContributionKeyBusinessProcess} }}",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].~wdTPcZMsI1(O[CostContributionKeyBusinessProcess] = \"10\" AND ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\")"
        )]
        [InlineData(
            "query {application(filter:{iTOwner_PersonSystem_some: {name:\"nom de la personne\"}}) {id name iTOwner_PersonSystem {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~ic5nTMC6H9fC\" AND ~L2000000Ca80[AssignedPerson]:~T20000000s10[PersonSystem].(~210000000900[Name] = \"nom de la personne\"))"
        )]
        [InlineData(
            "query {application(filter:{applicationOwner_PersonSystem_some:{email_contains:\"webeval\"}}) {id name}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~WzF2lb0yGb2U\" AND ~L2000000Ca80[AssignedPerson]:~T20000000s10[PersonSystem].(~Sy64inney0Y5[Email] Like \"#webeval#\"))"
        )]
        [InlineData(
            "query {application(filter:{and: {iTOwner_PersonSystem_some:{name: \"Louis\"} businessOwner_PersonSystem_some:{name:\"Dan\"}}}){id}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~ic5nTMC6H9fC\" AND ~L2000000Ca80[AssignedPerson]:~T20000000s10[PersonSystem].(~210000000900[Name] = \"Louis\")) AND ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~fd5niMC6H1iC\" AND ~L2000000Ca80[AssignedPerson]:~T20000000s10[PersonSystem].(~210000000900[Name] = \"Dan\"))"
        )]
        [InlineData(
            "query { application { businessProcess(filter: {costContributionKeyBusinessProcess_lt: 10}) { id}}}",
            "SELECT ~pj)grmQ9pG90[BusinessProcess] WHERE ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].~wdTPcZMsI1(O[CostContributionKeyBusinessProcess] < \"10\" AND ~i4n)MzlZpO00[Application]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\")"
        )]
        [InlineData(
            "query { application { businessCapability(filter: {link1CostContributionKeyRealization_lt: 10}) { id link1CostContributionKeyRealization}}}",
            "SELECT ~IcfsZhjW9T90[BusinessCapability] WHERE ~nd9L6Ys9Bvf1[BusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].~)W0hMxQOMPjU[CostContributionKeyRealization] < \"10\" AND ~nd9L6Ys9Bvf1[BusinessCapabilityFulfillment]:~Cd9LMSs9BnE1[BusinessCapabilityFulfillment].(~IdRdxwf6ET5P[FulfillingEnterpriseAgent]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"application-100\"))"
        )]
        [InlineData(
            "query {personSystem { applicationOwner_SoftwareInstallation(filter: {link1ResponsibilityAssignmentName_contains: \"responsibility name\"}) { link1ResponsibilityAssignmentName }}}",
            "SELECT ~x)XO7rMZFXj4[SoftwareInstallation] WHERE ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~WzF2lb0yGb2U\" AND ~oI2N0pLfGjhP[ResponsibilityAssignmentName] Like \"#responsibility name#\") AND ~gCr81RIpErMH[PersonAssignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~WzF2lb0yGb2U\" AND ~L2000000Ca80[AssignedPerson]:~T20000000s10[PersonSystem].(~310000000D00[Absolute Identifier] = \"personsystem-100\"))"
        )]
        [InlineData(
            "query {portfolio(filter:{id: \"3i2lutR)VfV4\" or: [{ portfolioType: ApplicationInventory  }, { portfolio_Lower_some: [{ id_empty: false }] } ]}, skip: 0, first: 10) { id name }}",
            "SELECT ~8A8iK3B1ETM6[Portfolio] WHERE (~XqRLVWr8HLzJ[PortfolioType] = \"I\" OR ~YrbsDojUFLAL[SubPortfolio]:~8A8iK3B1ETM6[Portfolio].(~310000000D00[Absolute Identifier] Is Not Null)) AND ~310000000D00[Absolute Identifier] = \"3i2lutR)VfV4\""
        )]
        public async Task Query_with_filter(string query, params string[] expectedERQLs)
        {
            await ExecuteQueryAsync(@query);
            var erqls = (ws.GetMegaRoot() as ISupportsDiagnostics).GeneratedERQLs;
            erqls.Should().BeEquivalentTo(expectedERQLs);
        }

        [Theory]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT = 0 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT = 1"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_not:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT Not= 0"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_not:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT Not= 1 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_lt:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT < 0"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_lt:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT < 1 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_lte:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT <= 0 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_lte:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT <= 1 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_gt:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT > 0"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_gt:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT > 1"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_gte:0}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT >= 0 OR ~h4n)MzlZpK00[BusinessProcess] Is Null"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count: {count_gte:1}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE ~h4n)MzlZpK00[BusinessProcess] HAVING COUNT >= 1"
        )]
        [InlineData(
            "query {application(filter:{businessProcess_count:{count_gte:0 count_not:3}}) {id name businessProcess {id name}}}",
            "SELECT ~MrUiM9B5iyM0[Application] WHERE (~h4n)MzlZpK00[BusinessProcess] HAVING COUNT Not= 3 OR ~h4n)MzlZpK00[BusinessProcess] Is Null) AND (~h4n)MzlZpK00[BusinessProcess] HAVING COUNT >= 0 OR ~h4n)MzlZpK00[BusinessProcess] Is Null)"
        )]
        public async Task Query_relations_with_having_count_filter(string query, params string[] expectedERQLs)
        {
            await ExecuteQueryAsync(@query);
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
        public async Task Get_schemas_containing_application()
        {
            var resp = await ExecuteQueryAsync(@"{metaClass {name schemas {name} }}", "metamodel");
            resp.Should().MatchGraphQL("data.metaClass", "*");
        }

        [Fact]
        public async Task Query_an_object_with_filter()
        {
            var resp = await ExecuteQueryAsync("query { businessDocument(filter:{id: \"MU4NHnrRUr0R\"}) { id }}");
            var collection = await ws.DataModel.GetCollectionAsync("BusinessDocument");
            resp.Should().ContainsGraphQLCount("data.businessDocument", collection.Count());
        }

        [Fact]
        public async Task Query_a_relationship()
        {
            var resp = await ExecuteQueryAsync(@"{application {name businessCapability {name id } } }");
            resp.Should().MatchGraphQL("data.application[0].businessCapability[0].name", "*");
        }

        [Theory]
        [InlineData("query { businessLine { id name application { name costContributionKey}}}", "data.businessLine")]
        [InlineData("query { application { id name businessLine {id name costContributionKey }}}", "data.application")]
        public async Task Query_with_specific_link_attribute(string query, string pattern)
        {
            var resp = await ExecuteQueryAsync(query);
            resp.Should().MatchGraphQL(pattern, "*");
        }

        [Fact]
        public async Task Query_an_object_diagrams()
        {
            var resp = await ExecuteQueryAsync(@"{application {id diagram {id} } }");

            resp.Should().MatchGraphQL("data.application[0].diagram[0].id", "*");
        }

        [Theory]
        [InlineData("ITPM", "diagram", "*/diagram/*/image")]
        [InlineData("ITPM", "businessDocument", "*/attachment/*/file")]
        [InlineData("ITPM", "businessDocumentVersion", "*/attachment/*/file")]
        [InlineData("MetaModel", "systemDiagram", "*/diagram/*/image")]
        [InlineData("MetaModel", "systemBusinessDocument", "*/attachment/*/file")]
        [InlineData("MetaModel", "systemBusinessDocumentVersion", "*/attachment/*/file")]
        public async Task Return_download_url_for_some_types(string schema, string type, string expectedPatern)
        {
            var resp = await ExecuteQueryAsync($@"{{{type} {{id downloadUrl}}}}", schema);

            resp.Should().MatchGraphQL($"data.{type}[0].downloadUrl", expectedPatern);
        }

        [Theory]
        [InlineData("ITPM", "businessDocument", "*/attachment/*/file")]
        [InlineData("MetaModel", "systemBusinessDocument", "*/attachment/*/file")]
        public async Task Return_upload_url_for_documents(string schema, string type, string expectedPatern)
        {
            var resp = await ExecuteQueryAsync($@"{{{type} {{id uploadUrl}}}}", schema);

            resp.Should().MatchGraphQL($"data.{type}[0].uploadUrl", expectedPatern);
        }

        [Theory]
        [InlineData("ITPM", "businessDocumentVersion")]
        [InlineData("Metamodel", "systemBusinessDocumentVersion")]
        public async Task Not_have_an_upload_url_on_document_version(string schema, string type)
        {
            var resp = await ExecuteQueryAsync($@"{{{type} {{id uploadUrl}}}}", schema);

            resp.Should().MatchGraphQL("errors[0].message", "Cannot query field 'uploadUrl'*");
        }

        [Fact]
        public async Task APIDiagnostic_should_return_values()
        {
            var result = await ExecuteQueryAsync("query { _APIdiagnostic { platformBuild } }");
            result.StatusCode.Should().Be(200);
        }

        private async Task<HopexResponse> ExecuteQueryAsync(string query, string schema = "ITPM")
        {
            ws = new MockEntryPoint(schema, 1);
            return await ws.Execute(new InputArguments
            {
                Query = query
            });
        }
    }
}
