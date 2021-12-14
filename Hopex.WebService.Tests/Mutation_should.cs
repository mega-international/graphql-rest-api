using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API.Library;
using System.Threading.Tasks;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class Mutation_should : MockRootBasedFixture
    {
        [Fact]
        public async Task Create_an_application()
        {
            var root = new MockMegaRoot.Builder().Build();

            var query = @"mutation { application: createApplication (application : { name:""test""} ) { id name }}";
            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.application.name", "test");
        }

        [Fact]
        public async Task Enforce_maxLength()
        {
            var root = new MockMegaRoot.Builder().Build();

            var nameTooLong = new string('x', 1025);
            var query = $@"mutation {{ application: createApplication (application : {{ name:""{nameTooLong}""}} ) {{id name }}}}";

            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().ContainsGraphQL("errors[0].message", $"Value {nameTooLong} for Name exceeds maximum length of 1024");
            resp.Should().MatchNull("data.application.name");
        }

        [Fact]
        public async Task Create_an_application_with_relationShips()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~abcdefghijkl", MetaClassLibrary.SoftwareTechnology))
                .Build();

            var query = @"mutation { application: createApplication ( application : {
                    name:""test""
                    softwareTechnology_UsedTechnology: {
                        action: add list: [{id:""~abcdefghijkl""}]
                    } } )
                {id name softwareTechnology_UsedTechnology {id}}}";

            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.application.name", "test");
            resp.Should().MatchGraphQL("data.application.softwareTechnology_UsedTechnology[0].id", "abcdefghijkl");
        }

        [Fact]
        public async Task Create_an_application_and_set_costContributionKey_on_new_businessCapability()
        {
            var root = new MockMegaRoot.Builder().Build();

            var query = @"mutation { 
                application: createApplication ( application : {
                    name:""application de test""
                    businessCapability: { action: add list: [{name:""business capability de test"" link1CostContributionKeyRealization: 77}] }
                } )
                { name businessCapability {name link1CostContributionKeyRealization} }}";

            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.application.businessCapability[0].name", "business capability de test");
            resp.Should().MatchGraphQL("data.application.businessCapability[0].link1CostContributionKeyRealization", "77");
        }

        [Fact]
        public async Task Create_an_application_and_set_costContributionKey_on_existing_businessCapability()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~abcdefghijkl", MetaClassLibrary.BusinessCapability))
                .Build();

            var query = @"mutation { 
                application: createApplication ( application : {
                    name:""application de test""
                    businessCapability: { action: add list: [{id: ""~abcdefghijkl"" name:""business capability de test"" link1CostContributionKeyRealization: 55}] }
                } )
                { name businessCapability {name link1CostContributionKeyRealization} }}";

            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.application.businessCapability[0].name", "business capability de test");
            resp.Should().MatchGraphQL("data.application.businessCapability[0].link1CostContributionKeyRealization", "55");
        }

        [Fact]
        public async Task Create_an_application_and_set_costContributionKey_on_existing_businessCapability_already_linked()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~mnopqrstuvwx", MetaClassLibrary.Application)
                .WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.ClassOfEnterpriseAgentExternalStructure_OwnedBusinessCapabilityFulfillment)
                    .WithChildren(new MockMegaObject("~abcdefghijkm", MetaClassLibrary.BusinessCapabilityFulfillment)
                    .WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.BusinessCapabilityFulfillment_FulfilledBusinessCapability)
                        .WithChildren(new MockMegaObject("~abcdefghijkl", MetaClassLibrary.BusinessCapability))))))
                .Build();

            var query = @"mutation { 
                application: updateApplication ( id: ""mnopqrstuvwx"" idType: INTERNAL application : {
                    name:""application de test""
                    businessCapability: {
                        action: add
                        list: [{id: ""~abcdefghijkl"" idType: INTERNAL name:""business capability de test"" link1CostContributionKeyRealization: 55}] }
                } )
                { name businessCapability {name link1CostContributionKeyRealization} }}";

            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.application.businessCapability[0].name", "business capability de test");
            resp.Should().MatchGraphQL("data.application.businessCapability[0].link1CostContributionKeyRealization", "55");
        }

        [Fact]
        public async void Create_an_application_with_deployment_date_in_specific_timezone()
        {
            var root = new MockMegaRoot.Builder().Build();
            var query = @"
                mutation
                {
                    createApplication(application:{name:""test"" deploymentDate: ""2020/12/31 00:00:00 +02:00""})
                    {
                        deploymentDate @date(format:""yyyy/MM-dd HH:mm:ss"")
                    }
                }";
            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().MatchGraphQL("data.createApplication.deploymentDate", "2020/12-30 22:00:00");
        }

        [Fact]
        public async void Mutation_on_readonly_relationship_should_be_forbidden()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~mnopqrstuvwx", MetaClassLibrary.ExchangeContract))
                .Build();

            var query = @"mutation
                {
                    updateExchangeContract(id: ""~mnopqrstuvwx""
                        exchangeContract:{
                            servicePoint_EnterpriseServicePort: {
                                action: ADD
                                list: [{name: ""forbidden instance""}]
                            }
                        }
                    )
                    {
                        id servicePoint_EnterpriseServicePort{ name }
                    }
                }";
            var resp = await ExecuteQueryAsync(root, query, "BPA");
            resp.Should().ContainsGraphQLCountGreaterThan("errors", 0);
        }

        [Fact]
        public async void Mutation_on_path_property_from_linked_item_context_should_work()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~mnopqrstuvwx", MetaClassLibrary.Application)
                .WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.Application_BusinessProcess)
                    .WithChildren(new MockMegaObject("~abcdefghijkm", MetaClassLibrary.BusinessProcess))))
                .Build();

            var query = @"mutation
                {
                    updateApplication(id: ""~mnopqrstuvwx""
                        application:{
                            businessProcess: {
                                action: ADD
                                list: [{id: ""abcdefghijkm"" costContributionKeyBusinessProcess: 66}]
                            }
                        }
                    )
                    {
                        id businessProcess { costContributionKeyBusinessProcess }
                    }
                }";
            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.updateApplication.businessProcess[0].costContributionKeyBusinessProcess", "66");
        }
    }
}
