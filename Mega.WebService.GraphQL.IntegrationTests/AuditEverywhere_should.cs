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
    static class IDs
    {
        public const string AUDIT_ACTIVITY_INTERNAL = "EtC)y503FPyP";
        public const string FINDING_EXTERNAL = "isFuAMoxBZkWxjAbue7j";
        public const string RECOMMENDATION_EXTERNAL = "42QJr1SEoE7Oe6F0cVG7";
        public const string BUSINESS_DOCUMENT_EXTERNAL = "u8rY2kM36IZkG8FBBwwH";
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    public class AuditEverywhere_should : BaseFixture, IClassFixture<AuditEverywhereFixture>
    {
        private GraphQLHttpClient _graphQLClient;

        public AuditEverywhere_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _graphQLClient = await _fx.GetGraphQLClientAsync("Audit");
        }

        [Fact]
        public async Task Create_an_audit_activity()
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation {
                            createUpdateAuditActivity( id:""" + IDs.AUDIT_ACTIVITY_INTERNAL + @""" idType:INTERNAL auditActivity: {
                                name: ""TestName""
                                beginDate: ""2019-10-11""
                                activityEndDate: ""2019-11-12""
                                estimatedWorkloadHours: 15
                                computedActivityEffectiveWorkloadHours: 20
                                comment: ""New description &éô&\nnew line""
                            }) {
                                id
                                name
                                estimatedWorkloadHours
                                activityEffectiveWorkloadHours
                                beginDate
                                activityEndDate
                                comment(format:HTML)
                            }
                          }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateUpdateAuditActivityResponse>(request);

            response.Should().HaveNoError();
            var activity = response.Data.CreateUpdateAuditActivity;
            activity.Should().BeEquivalentTo(new AuditActivity
            {
                Id = IDs.AUDIT_ACTIVITY_INTERNAL,
                Name = "TestName",
                EstimatedWorkloadHours = 15.0,
                ActivityEffectiveWorkloadHours = 20.0,
                BeginDate = "2019-10-11",
                ActivityEndDate = "2019-11-12",
                Comment = "New description &éô&\nnew line" // TODO: what is doing the HTML export
            });
        }

        [Fact]
        public async Task<Finding> Create_a_finding_on_an_activity()
        {
            await Create_an_audit_activity();
            var request = new GraphQLRequest()
            {
                Query = @"mutation {
                            createUpdateFinding(
                                id: """ + IDs.FINDING_EXTERNAL + @""" idType:EXTERNAL
                                finding:{
                                        name: ""New finding offline 1305 H0838""
                                        findingImpact: VeryHigh
                                        findingType: Weakness
                                        detailedDescription: ""<div>With details</div>""
                                        auditActivity_Activity: { action: ADD list:[{id:""" + IDs.AUDIT_ACTIVITY_INTERNAL + @"""}]}
                                }
                            ){
                                id
                                externalId
                                name
                                findingImpact
                                findingType
                                detailedDescription
                              }
                          }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateUpdateFindingResponse>(request);

            response.Should().HaveNoError();
            var finding = response.Data.CreateUpdateFinding;
            finding.Id.Should().NotBeNullOrEmpty();
            finding.Should().BeEquivalentTo(new Finding
            {
                ExternalId = IDs.FINDING_EXTERNAL,
                Name = "New finding offline 1305 H0838",
                FindingImpact = "VeryHigh",
                FindingType = "Weakness",
                DetailedDescription = "<div>With details</div>"
            }, options => options.Excluding(f => f.Id));
            return finding;
        }

        [Fact]
        public async Task<(Finding, Recommendation)> Create_a_recommendation_on_a_finding()
        {
            var finding = await Create_a_finding_on_an_activity();
            var request = new GraphQLRequest()
            {
                Query = @"mutation {
                            createUpdateRecommendation(
                                id:""" + IDs.RECOMMENDATION_EXTERNAL + @""" idType:EXTERNAL
                                recommendation:{
                                    name: ""new reco MII""
                                    recommendationPriority: VeryHigh
                                    details:""Details newly created recommendation""
                                    finding: { action: ADD list:[{id:""" + finding.Id + @""" idType:INTERNAL}] }
                                }
                            ) {
                                id
                                externalId
                                name
                                details
                            }
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateUpdateRecommendationResponse>(request);

            response.Should().HaveNoError();
            var recommendation = response.Data.CreateUpdateRecommendation;
            recommendation.Id.Should().NotBeNull();
            recommendation.Should().BeEquivalentTo(new Recommendation
            {
                ExternalId = IDs.RECOMMENDATION_EXTERNAL,
                Name = "new reco MII",
                Details = "Details newly created recommendation"
            }, options => options.Excluding(r => r.Id));
            return (finding, recommendation);
        }

        [Fact]
        public async Task Create_an_audit_evidence_document()
        {
            await Create_a_finding_on_an_activity();
            var request = new GraphQLRequest()
            {
                Query = @"mutation{
                            createUpdateBusinessDocument( id:""" + IDs.BUSINESS_DOCUMENT_EXTERNAL + @""" idType:EXTERNAL
                                businessDocument:{
                                    name: ""test MII 1234568""
                                    finding_Object: { action: ADD list:[{id:""" + IDs.FINDING_EXTERNAL + @""" idType:EXTERNAL}] }
                                    documentCategory:{ action:ADD list:[{id:""g4InWZSRGDBB"" idType:INTERNAL}] }
                                    businessDocumentPattern_DocumentPattern:{ action:ADD list:[{id:""KoQoM0jRGLZH"" idType:INTERNAL}] }
                            }) { id } }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateUpdateBusinessDocumentResponse>(request);

            response.Should().HaveNoError();
            response.Data.CreateUpdateBusinessDocument.Id.Should().NotBeNull();            
        }

        [Fact]
        public async Task Delete_a_finding_but_not_its_recommendation()
        {
            var (finding, recommendation) = await Create_a_recommendation_on_a_finding();
            var request = new GraphQLRequest()
            {
                Query = @"mutation{ deleteFinding(id:""" + finding.Id + @""" idType:INTERNAL cascade:true) { deletedCount }}"
            };

            var response = await _graphQLClient.SendQueryAsync<DeleteFindingResponse>(request);

            response.Should().HaveNoError();
            await RecommendationShouldBeInRepository(recommendation);
        }

        [Fact]
        public async Task Delete_an_audit_activity_and_its_finding_but_not_its_recommendation()
        {
            var (finding, recommendation) = await Create_a_recommendation_on_a_finding();
            var request = new GraphQLRequest()
            {
                Query = @"mutation{ deleteAuditActivity(id:""" + IDs.AUDIT_ACTIVITY_INTERNAL + @""" idType:INTERNAL cascade:true) { deletedCount }}"
            };

            var response = await _graphQLClient.SendQueryAsync<DeleteAuditActivityResponse>(request);

            response.Should().HaveNoError();
            await FindingShouldNotBeInRepository(finding);
            await RecommendationShouldBeInRepository(recommendation);
        }

        private async Task FindingShouldNotBeInRepository(Finding finding)
        {
            var request = new GraphQLRequest()
            {
                Query = @"query { finding(filter:{id:""" + finding.Id + @"""}) { id } }"
            };

            var response = await _graphQLClient.SendQueryAsync<FindingResponse>(request);

            response.Should().HaveNoError();
            response.Data.Finding.Should().BeEmpty();
        }

        private async Task RecommendationShouldBeInRepository(Recommendation recommendation)
        {
            var request = new GraphQLRequest()
            {
                Query = @"query { recommendation(filter:{id:""" + recommendation.Id + @"""}) { id } }"
            };

            var response = await _graphQLClient.SendQueryAsync<RecommendationResponse>(request);

            response.Should().HaveNoError();
            response.Data.Recommendation.Should().BeEquivalentTo(new List<Recommendation> { recommendation }, options => options.Including( r => r.Id));
        }

        public class CreateUpdateAuditActivityResponse
        {
            public AuditActivity CreateUpdateAuditActivity { get; set; }
        }

        public class CreateUpdateFindingResponse
        {
            public Finding CreateUpdateFinding { get; set; }
        }

        public class CreateUpdateRecommendationResponse
        {
            public Recommendation CreateUpdateRecommendation { get; set; }
        }

        public class CreateUpdateBusinessDocumentResponse
        {
            public BasicObject CreateUpdateBusinessDocument { get; set; }
        }

        public class DeleteAuditActivityResponse
        {
            public DeleteResult DeleteAuditActivity { get; set; }
        }

        public class DeleteFindingResponse
        {
            public DeleteResult DeleteFinding { get; set; }
        }

        public class FindingResponse
        {
            public List<Finding> Finding { get; set; }
        }

        public class RecommendationResponse
        {
            public List<Recommendation> Recommendation { get; set; }
        }

        public class AuditActivity : BasicObject
        {
            public double EstimatedWorkloadHours { get; set; }
            public double ActivityEffectiveWorkloadHours { get; set; }
            public string BeginDate { get; set; }
            public string ActivityEndDate { get; set; }
            public string DetailedDescription { get; set; }            
        }

        public class Finding : BasicObject
        {
            public string FindingImpact { get; set; }
            public string FindingType { get; set; }
            public string DetailedDescription { get; set; }
        }

        public class Recommendation : BasicObject
        {
            public string Details { get; set; }
        }
    }

    [Collection("Global")]
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    public class AuditEverywhereFixture : IAsyncLifetime
    {
        private GlobalFixture _fx;
        public AuditEverywhereFixture(GlobalFixture fx)
        {
            _fx = fx;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var client = await _fx.GetGraphQLClientAsync("Audit");
            var request = new GraphQLRequest()
            {
                Query = @"mutation deleteAll {                            
                            deleteAuditActivity(id:""" + IDs.AUDIT_ACTIVITY_INTERNAL + @""" idType:INTERNAL) { deletedCount }
                            deleteFinding(id:""" + IDs.FINDING_EXTERNAL + @""" idType:EXTERNAL) { deletedCount }
                            deleteRecommendation(id: """ + IDs.RECOMMENDATION_EXTERNAL + @""" idType: EXTERNAL) { deletedCount }
                            deleteBusinessDocument(id: """ + IDs.BUSINESS_DOCUMENT_EXTERNAL + @""" idType: EXTERNAL) { deletedCount }                            
                          }"
            };
            var response = await client.SendQueryAsync<DeleteAllResponse>(request);
            var data = response.Data;
            data.DeleteAuditActivity?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteFinding?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteRecommendation?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBusinessDocument?.DeletedCount.Should().BeGreaterOrEqualTo(0);
        }

        public class DeleteAllResponse
        {
            public DeleteResult DeleteAuditActivity { get; set; }
            public DeleteResult DeleteFinding { get; set; }
            public DeleteResult DeleteRecommendation { get; set; }
            public DeleteResult DeleteBusinessDocument { get; set; }
        }
    }
}
