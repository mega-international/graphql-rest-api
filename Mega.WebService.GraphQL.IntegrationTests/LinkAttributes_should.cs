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
    public class LinkAttributes_should : BaseFixture
    {
        private GraphQLHttpClient _graphQLClient;

        public LinkAttributes_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            await CreateAllTestObjects();
        }

        [Fact]
        public async void Update_order_link_attribute()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation updateBusinessProcess {
                            updateAppBusinessProcess: updateApplication(id: ""TEST_OGD_APP1"" idType: EXTERNAL application: {
                                name: ""App update via externalID""
                                comment: ""comment step 2 update""
                                businessProcess: {
                                    action: ADD
                                    list:[{id:""TEST_OGD_BP1"" idType: EXTERNAL, order:50}]
                                }
                            }) {
                                name
                                comment
                                businessProcess { name order }
                            }
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<UpdateAppBusinessProcessResponse>(request);

            response.Should().HaveNoError();
            response.Data.UpdateAppBusinessProcess.Should().BeEquivalentTo(
                new ApplicationInstance
                {
                    Name = "App update via externalID",
                    Comment = "comment step 2 update",
                    BusinessProcess = new List<OrderedBasicObject>
                    {
                        new OrderedBasicObject { Name = "TEST OGD BP1", Order = 50 }
                    }
                });
        }

        [Fact]
        public async void Update_cost_contribution()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation {
                            updateAppBusinessLine: updateApplication (id:""TEST_OGD_APP1"" idType:EXTERNAL application : {
                                name : ""Account graphqL""
                                cloudComputing: Cloud_IaaS
                                comment : ""update via graphQL""
                                businessLine: {
                                    action: ADD
                                    list:[
                                        {id:""TEST_OGD_BL1"", idType: EXTERNAL, order:10, costContributionKey:50},
                                        {id:""TEST_OGD_BL2"", idType: EXTERNAL, order:20, costContributionKey:30},
                                        {id:""TEST_OGD_BL3"", idType: EXTERNAL, order:30, costContributionKey:20}
                                    ]
                                }
                             }) {
                                name cloudComputing comment
                                businessLine { name order costContributionKey }
                             }
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<UpdateAppBusinessLineResponse>(request);

            response.Should().HaveNoError();
            response.Data.UpdateAppBusinessLine.Should().BeEquivalentTo(
                new ApplicationInstance
                {
                    Name = "Account graphqL",
                    CloudComputing = "Cloud_IaaS",
                    Comment = "update via graphQL",
                    BusinessLine = new List<OrderedBusinessLine>
                    {
                        new OrderedBusinessLine { Name = "BL1", Order = 10, CostContributionKey = 50 },
                        new OrderedBusinessLine { Name = "BL2", Order = 20, CostContributionKey = 30 },
                        new OrderedBusinessLine { Name = "BL3", Order = 30, CostContributionKey = 20 }
                    }
                });
        }

        private async Task<CreateAllResponse> CreateAllTestObjects()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation createAll {
                            businessProcess: createBusinessProcess(id: ""TEST_OGD_BP1"" idType: EXTERNAL businessProcess:{
                                name: ""TEST OGD BP1""
                                comment: ""created automatically with external ID TEST_OGD_BP1""
                            }) { externalId }
                            application: createApplication(id: ""TEST_OGD_APP1"" idType: EXTERNAL application:{
                                name: ""OGD App 1""
                                businessProcess: {
                                    action: ADD
                                    list:[{id:""TEST_OGD_BP1"", idType:EXTERNAL}]
                                }
                            }) { externalId } 
                            BusinessLine1:createBusinessLine(id:""TEST_OGD_BL1"" idType:EXTERNAL businessLine:{
                                name: ""BL1""
                            }) { externalId }
                            BusinessLine2:createBusinessLine(id:""TEST_OGD_BL2"" idType:EXTERNAL businessLine:{
                                name: ""BL2""
                            }) { externalId }
                            BusinessLine3:createBusinessLine(id:""TEST_OGD_BL3"" idType:EXTERNAL businessLine:{
                                name: ""BL3""
                            }) { externalId }
                        }"
            };
              
            var response = await _graphQLClient.SendQueryAsync<CreateAllResponse>(request);
            var data = response.Data;
            data.BusinessProcess.ExternalId.Should().Be("TEST_OGD_BP1");
            data.Application.ExternalId.Should().Be("TEST_OGD_APP1");
            data.BusinessLine1.ExternalId.Should().Be("TEST_OGD_BL1");
            data.BusinessLine2.ExternalId.Should().Be("TEST_OGD_BL2");
            data.BusinessLine3.ExternalId.Should().Be("TEST_OGD_BL3");
            return data;
        }

        public override async Task DisposeAsync()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation deleteAll
                        {                            
                            deletaApp: deleteApplication(id:""TEST_OGD_APP1"" idType:EXTERNAL) { deletedCount }
                            deleteBP: deleteBusinessProcess(id: ""TEST_OGD_BP1"" idType: EXTERNAL) { deletedCount }
                            deleteBL1: deleteBusinessLine(id: ""TEST_OGD_BL1"" idType: EXTERNAL) { deletedCount }
                            deleteBL2: deleteBusinessLine(id: ""TEST_OGD_BL2"" idType: EXTERNAL) { deletedCount }
                            deleteBL3: deleteBusinessLine(id: ""TEST_OGD_BL3"" idType: EXTERNAL) { deletedCount }
                         }"
            };
            var response = await _graphQLClient.SendQueryAsync<DeleteAllResponse>(request);
            var data = response.Data;
            data.DeleteApp?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBP?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBL1?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBL2?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBL3?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            await base.DisposeAsync();            
        }

        public class CreateAllResponse
        {
            public BasicObject BusinessProcess { get; set; }
            public BasicObject Application { get; set; }
            public BasicObject BusinessLine1 { get; set; }
            public BasicObject BusinessLine2 { get; set; }
            public BasicObject BusinessLine3 { get; set; }
        }

        public class UpdateAppBusinessProcessResponse
        {
            public ApplicationInstance UpdateAppBusinessProcess { get; set; }
        }

        public class UpdateAppBusinessLineResponse
        {
            public ApplicationInstance UpdateAppBusinessLine { get; set; }
        }

        public class DeleteAllResponse
        {            
            public DeleteResult DeleteApp { get; set; }
            public DeleteResult DeleteBP { get; set; }
            public DeleteResult DeleteBL1 { get; set; }
            public DeleteResult DeleteBL2 { get; set; }
            public DeleteResult DeleteBL3 { get; set; }
        }

        public class ApplicationInstance : BasicObject
        {
            public string CloudComputing { get; set; }
            public List<OrderedBasicObject> BusinessProcess { get; set; }
            public List<OrderedBusinessLine> BusinessLine { get; set; }
        }        

        public class OrderedBusinessLine : OrderedBasicObject
        {
            public double CostContributionKey { get; set; }
        }
    }
}
