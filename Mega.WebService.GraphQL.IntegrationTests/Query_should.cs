using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    // ReSharper disable once InconsistentNaming
    public class Query_should : BaseFixture
    {
        // ReSharper disable once InconsistentNaming
        private GraphQLHttpClient _graphQLClient;

        public Query_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            await DisposeAsync();
            await CreateAllTestObjects();
        }

        private async Task CreateAllTestObjects()
        {
            var requestCreate = new GraphQLRequest
            {
                Query = @"mutation createAll
                        {
                            application1:createApplication(application:{name:""Name of the application 1""}){id name}
                            application2:createApplication(application:{name:""Name of the application 2""}){id name}
                            softwareTechnology1:createSoftwareTechnology(softwareTechnology:{name:""Name of the software technology 1""}){id name}
                            softwareTechnology2:createSoftwareTechnology(softwareTechnology:{name:""Name of the software technology 2""}){id name}
                        }"
            };
            var responseCreate = await _graphQLClient.SendQueryAsync<CreateAllResponse>(requestCreate);
            responseCreate.Data.Application1.Name.Should().Be("Name of the application 1");
            responseCreate.Data.Application2.Name.Should().Be("Name of the application 2");
            responseCreate.Data.SoftwareTechnology1.Name.Should().Be("Name of the software technology 1");
            responseCreate.Data.SoftwareTechnology2.Name.Should().Be("Name of the software technology 2");
            var requestUpdate = new GraphQLRequest
            {
                Query = $@"mutation updateApplication
                        {{
                            application1:updateApplication(
                                id:""{responseCreate.Data.Application1.Id}""
                                application:
                                {{
                                    softwareTechnology_UsedTechnology:
                                    {{
                                        action:ADD list:[{{id:""{responseCreate.Data.SoftwareTechnology1.Id}""}}, {{id:""{responseCreate.Data.SoftwareTechnology2.Id}""}}]
                                    }}
                                }})
                            {{id name}}
                        }}"
            };
            var responseUpdate = await _graphQLClient.SendQueryAsync<CreateAllResponse>(requestUpdate);
            responseUpdate.Should().HaveNoError();
        }

        [Fact]
        public async void Count_applications()
        {
            var request = new GraphQLRequest
            {
                Query = @"query applicationAggregatedValues
                        {
                            applicationAggregatedValues(filter:{name_contains: ""Name of the application""}){id(function:COUNT)}
                        }"
            };
            var response = await _graphQLClient.SendQueryAsync<AggregationQuery>(request);
            response.Should().HaveNoError();
            response.Data.ApplicationAggregatedValues[0].Id.Should().Be(2);
        }

        [Fact]
        public async void Count_applications_softwareTechnology_UsedTechnology()
        {
            var request = new GraphQLRequest
            {
                Query = @"query application_softwareTechnology_UsedTechnologyAggregatedValues
                        {
                            application(filter:{name_contains:""Name of the Application""})
                            {
                                softwareTechnology_UsedTechnologyAggregatedValues
                                {
                                    id(function:COUNT)
                                }
                            }
                        }"
            };
            var response = await _graphQLClient.SendQueryAsync<Application_softwareTechnology_UsedTechnologyAggregatedValues>(request);
            response.Should().HaveNoError();
            response.Data.Application[0].SoftwareTechnology_UsedTechnologyAggregatedValues[0].Id.Should().Be(2);
        }

        public override async Task DisposeAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"mutation deleteAll
                        {
                            deleteApplication: deleteManyApplication(filter:{name_contains: ""Name of the application""}) { deletedCount }
                            deleteSoftwareTechnology: deleteManySoftwareTechnology(filter:{name_contains: ""Name of the software technology""}) { deletedCount }
                        }"
            };
            var response = await _graphQLClient.SendQueryAsync<DeleteAllResponse>(request);
            var data = response.Data;
            data.DeleteApplication?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteSoftwareTechnology?.DeletedCount.Should().BeGreaterOrEqualTo(0);

            await base.DisposeAsync();            
        }

        public class CreateAllResponse
        {
            public BasicObject Application1 { get; set; }
            public BasicObject Application2 { get; set; }
            public BasicObject SoftwareTechnology1 { get; set; }
            public BasicObject SoftwareTechnology2 { get; set; }
        }

        public class DeleteAllResponse
        {
            public DeleteResult DeleteApplication { get; set; }
            public DeleteResult DeleteSoftwareTechnology { get; set; }
        }

        public class AggregationQuery
        {
            public List<AggregatedResult> ApplicationAggregatedValues { get; set; }
        }
        
        public class AggregatedResult
        {
            public double Id { get; set; }
        }

        public class Application_softwareTechnology_UsedTechnologyAggregatedValues
        {
            public List<ApplicationInstance> Application { get; set; }
        }

        public class ApplicationInstance : BasicObject
        {
            public List<AggregatedResult> SoftwareTechnology_UsedTechnologyAggregatedValues { get; set; }
        }
    }
}
