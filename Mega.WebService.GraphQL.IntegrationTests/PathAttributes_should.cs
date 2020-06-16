using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    public class PathAttributes_should : BaseFixture
    {
        private GraphQLHttpClient _graphQLClient;

        public PathAttributes_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            await CreateAllTestObjects();
        }

        [Fact]
        public async void Update_order_attribute_in_path_on_first_intermediate_object()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation {
                            updateAppCapability: updateApplication (id:""TEST_OGD_APP5"" idType:EXTERNAL application : {
                                name : ""App update via externalID""
                                comment: ""comment step 2 update""
                                businessCapability: {
                                    action: ADD
                                    list:[
                                        {id:""TEST_OGD_BC1""  idType:EXTERNAL order:50},
                                        {id:""TEST_OGD_BC2""  idType:EXTERNAL order:70}]
                                }
                            }) {
                                name
                                comment
                                businessCapability { name }
                                ownedBusinessCapabilityFulfillment: customRelationship(id: ""JdRdxwf6EX5P"") {
                                    order: customField(id: ""410000000H00"")
                                    fullfilledBusinessCapability: customRelationship(id: ""od9L6Ys9Bzf1"") { name: customField(id: ""210000000900"") }
                                }
                            }
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<UpdateAppBusinessCapabilityResponse>(request);

            response.Should().HaveNoError();
            var businessCapability1 = new OrderedBasicObject { Name = "TEST OGD BC1" };
            var businessCapability2 = new OrderedBasicObject { Name = "TEST OGD BC2" };
            response.Data.UpdateAppCapability.Should().BeEquivalentTo(
                new ApplicationInstance
                {
                    Name = "App update via externalID",
                    Comment = "comment step 2 update",
                    BusinessCapability = new List<OrderedBasicObject>{ businessCapability1, businessCapability2 },
                    OwnedBusinessCapabilityFulfillment = new List<Fulfillment>
                    {
                        new Fulfillment { Order = 50, FullfilledBusinessCapability = new List<OrderedBasicObject>{ businessCapability1 } },
                        new Fulfillment { Order = 70, FullfilledBusinessCapability = new List<OrderedBasicObject>{ businessCapability2 } }
                    }
                });
        }

        private async Task<CreateAllResponse> CreateAllTestObjects()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation createAll {
                            businessCapabilityBC1:createBusinessCapability(id:""TEST_OGD_BC1"" idType:EXTERNAL businessCapability:{
                                name: ""TEST OGD BC1""
                                comment: ""created automatically with external ID TEST_OGD_BC1""
                            }) { externalId }
                            businessCapabilityBC2:createBusinessCapability(id:""TEST_OGD_BC2"" idType:EXTERNAL businessCapability:{
                                name: ""TEST OGD BC2""
                                comment: ""created automatically with external ID TEST_OGD_BC2""
                            }) { externalId }
                            application:createApplication(id:""TEST_OGD_APP5"" idType:EXTERNAL application:{
                                name: ""OGD App 5""
                                businessCapability: {
                                    action: ADD
                                    list:[{id:""TEST_OGD_BC1"" idType:EXTERNAL}]
                                }
                           }) { externalId businessCapability { externalId } }  
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateAllResponse>(request);
            var data = response.Data;
            data.BusinessCapabilityBC1.ExternalId.Should().Be("TEST_OGD_BC1");
            data.BusinessCapabilityBC2.ExternalId.Should().Be("TEST_OGD_BC2");
            data.Application.ExternalId.Should().Be("TEST_OGD_APP5");
            data.Application.BusinessCapability.Should().BeEquivalentTo( new OrderedBasicObject { ExternalId = "TEST_OGD_BC1" });
            return data;
        }

        public override async Task DisposeAsync()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation deleteAll {                            
                            deletaApp: deleteApplication(id:""TEST_OGD_APP5"" idType:EXTERNAL) { id }
                            deleteBC1: deleteBusinessCapability(id: ""TEST_OGD_BC1"" idType: EXTERNAL) { id }
                            deleteBC2: deleteBusinessCapability(id: ""TEST_OGD_BC2"" idType: EXTERNAL) { id }
                        }"
            };
            var response = await _graphQLClient.SendQueryAsync<DeleteAllResponse>(request);
            var data = response.Data;
            data.DeleteApp.Should().BeNull();
            data.DeleteBC1.Should().BeNull();
            data.DeleteBC2.Should().BeNull();

            await base.DisposeAsync();
        }

        public class CreateAllResponse
        {
            public BasicObject BusinessCapabilityBC1 { get; set; }
            public BasicObject BusinessCapabilityBC2 { get; set; }
            public ApplicationInstance Application { get; set; }
        }

        public class UpdateAppBusinessCapabilityResponse
        {
            public ApplicationInstance UpdateAppCapability { get; set; }
        }

        public class ApplicationInstance : BasicObject
        {
            public List<OrderedBasicObject> BusinessCapability { get; set; }
            public List<Fulfillment> OwnedBusinessCapabilityFulfillment { get; set; }
        }

        public class DeleteAllResponse
        {
            public BasicObject DeleteApp { get; set; }
            public BasicObject DeleteBC1 { get; set; }
            public BasicObject DeleteBC2 { get; set; }            
        }

        public class Fulfillment : OrderedBasicObject
        {
            public List<OrderedBasicObject> FullfilledBusinessCapability { get; set; }
        }
    }
}
