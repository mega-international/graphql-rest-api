using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    [ImportMgr("QueryAssessmentNode.mgr")]
    public class GenericFields_should : BaseFixture
    {
        public GenericFields_should(GlobalFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        { }

        [Fact]
        public async void Query_assessment_node_answers()
        {
            var request = new GraphQLRequest()
            {
                Query = @"query {
                            assessmentNode(filter: {id: ""SXS9csIOUvrV"" }) {
                                id
                                enumCombo:  customField(id: ""oYS9NpIOULeV"")
                                text:  customField(id: ""HZS9LrIOUHoV"")
                                boolCheck: customField(id: ""zXS9yvIOUXwV"")
                                docCollection: customField(id: ""FXS9yxIOUf)V"")
                                docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                                    id
                                    customField(id: ""Z20000000D60"")
                                    ... on BusinessDocument {
                                        name
                                    }
                                }
                            }
                          }"
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");

            var response = await graphQLClient.SendQueryAsync<AssessmentNodesResponse>(request);

            response.Should().HaveNoError();
            var nodes = response.Data.AssessmentNode;
            nodes.Should().HaveCount(1);
            nodes[0].Id.Should().Be("SXS9csIOUvrV");
            nodes[0].EnumCombo.Should().Be("3");
            nodes[0].Text.Should().Be("Test text");
            nodes[0].BoolCheck.Should().Be("1");
            nodes[0].DocCollection.Should().Contain("[{\"id\":\"MYS9E)IOUr6W\",\"name\":\"QueryAssessmentNode_Doc\"}]");
            nodes[0].Docs[0].Id.Should().Be("MYS9E)IOUr6W");
            nodes[0].Docs[0].CustomField.Should().Be("QueryAssessmentNode_Doc");
            nodes[0].Docs[0].Name.Should().Be("QueryAssessmentNode_Doc");
        }

        [Fact]
        public async void Create_assessment_node_answers()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var createRequest = new GraphQLRequest()
            {
                Query = @"mutation {
                            createAssessmentNode(assessmentNode:{
                                customFields : [
                                    {id: ""oYS9NpIOULeV"", value: ""1""},
                                    {id: ""HZS9LrIOUHoV"", value: ""first text""},
                                    {id: ""zXS9yvIOUXwV"", value: ""0""}]
                         })
                        {
                            id
                            enumCombo:  customField(id: ""oYS9NpIOULeV"")
                            text:  customField(id: ""HZS9LrIOUHoV"")
                            boolCheck: customField(id: ""zXS9yvIOUXwV"")
                        }}"
            };

            var createResponse = await graphQLClient.SendQueryAsync<CreateAssessmentNodeResponse>(createRequest);

            createResponse.Should().HaveNoError();
            var node = createResponse.Data.CreateAssessmentNode;
            node.Id.Should().NotBeNullOrEmpty();
            node.EnumCombo.Should().Be("1");
            node.Text.Should().Be("first text");
            node.BoolCheck.Should().Be("0");
        }

        [Fact]
        public async void Update_assessment_node_answers()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var node = await CreateEmptyNode(graphQLClient);
            var updateRequest = new GraphQLRequest()
            {
                Query = @"mutation($id: String!) {
                            updateAssessmentNode(id: $id, assessmentNode:{
                                customFields: [
                                    {id: ""oYS9NpIOULeV"", value: ""3""},
                                    {id: ""HZS9LrIOUHoV"", value: ""second text""},
                                    {id: ""zXS9yvIOUXwV"", value: ""1""}],
                                customRelationships: [{
                                    action: ADD,
                                    relationId: ""FXS9yxIOUf)V"",
                                    list: [{id: ""MYS9E)IOUr6W""}]
                                }]
                                
                         }) {
                            enumCombo:  customField(id: ""oYS9NpIOULeV"")
                            text:  customField(id: ""HZS9LrIOUHoV"")
                            boolCheck: customField(id: ""zXS9yvIOUXwV"")
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                                    id
                            }
                        } }",
                Variables = new { id = node.Id }
            };

            var updateResponse = await graphQLClient.SendQueryAsync<UpdateAssessmentNodeResponse>(updateRequest);

            updateResponse.Should().HaveNoError();
            node = updateResponse.Data.UpdateAssessmentNode;
            node.EnumCombo.Should().Be("3");
            node.Text.Should().Be("second text");
            node.BoolCheck.Should().Be("1");
            node.Docs[0].Id.Should().Be("MYS9E)IOUr6W");
        }

        [Fact]
        public async void Remove_assessment_node_document()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var node = await CreateNodeWithADocument(graphQLClient);
            var updateRequest = new GraphQLRequest()
            {
                Query = @"mutation($id: String!) {
                            updateAssessmentNode(id: $id, assessmentNode:{
                                customRelationships: [{
                                    action: REMOVE,
                                    relationId: ""FXS9yxIOUf)V"",
                                    list: [{id: ""MYS9E)IOUr6W""}]
                                }]
                                
                         }) {
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") { id }
                        } }",
                Variables = new { id = node.Id }
            };

            var updateResponse = await graphQLClient.SendQueryAsync<UpdateAssessmentNodeResponse>(updateRequest);

            updateResponse.Should().HaveNoError();
            node = updateResponse.Data.UpdateAssessmentNode;
            node.Docs.Should().BeEmpty();
        }

        [Fact]
        public async void Replace_assessment_node_document()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var node = await CreateNodeWithADocument(graphQLClient);
            var document = await CreateDocument(graphQLClient);

            var updateRequest = new GraphQLRequest()
            {
                Query = @"mutation($id: String!,$documentId: String!) {
                            updateAssessmentNode(id: $id, assessmentNode:{
                                customRelationships: [{
                                    action: REPLACE_ALL,
                                    relationId: ""FXS9yxIOUf)V"",
                                    list: [{id: $documentId}]
                                }]
                                
                         }) {
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") { id }
                        } }",
                Variables = new
                {
                    id = node.Id,
                    documentId = document.Id
                }
            };

            var updateResponse = await graphQLClient.SendQueryAsync<UpdateAssessmentNodeResponse>(updateRequest);

            updateResponse.Should().HaveNoError();
            node = updateResponse.Data.UpdateAssessmentNode;
            node.Docs.Should().HaveCount(1);
            node.Docs[0].Id.Should().Be(document.Id);
        }

        [Fact]
        public async void Upsert_an_inexisting_assessment_node_()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var document = await CreateDocument(graphQLClient);
            var externalId = Guid.NewGuid().ToString();
            var upsertRequest = new GraphQLRequest()
            {
                Query = @"mutation($id: String!,$documentId: String!) {
                            createUpdateAssessmentNode(
                                id: $id, idType: EXTERNAL,
                                assessmentNode:{
                                    customFields: [{id: ""HZS9LrIOUHoV"", value: ""upsert text""}],                                
                                    customRelationships: [{
                                        action: REPLACE_ALL,
                                        relationId: ""FXS9yxIOUf)V"",
                                        list: [{id: $documentId}]
                                    }]
                                
                         }) {
                            externalId: customField(id: ""CFmhlMxNT1iE"")
                            text: customField(id: ""HZS9LrIOUHoV"")
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") { id }
                        } }",
                Variables = new
                {
                    id = externalId,
                    documentId = document.Id
                }
            };

            var upsertResponse = await graphQLClient.SendQueryAsync<CreateUpdateAssessmentNodeResponse>(upsertRequest);

            upsertResponse.Should().HaveNoError();
            var node = upsertResponse.Data.CreateUpdateAssessmentNode;
            node.ExternalId.Should().Be(externalId);
            node.Text.Should().Be("upsert text");
            node.Docs.Should().HaveCount(1);
            node.Docs[0].Id.Should().Be(document.Id);
        }

        [Fact]
        public async void Upsert_an_existing_assessment_node_()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Assessment");
            var node = await CreateNodeWithADocument(graphQLClient);
            var document = await CreateDocument(graphQLClient);
            var upsertRequest = new GraphQLRequest()
            {
                Query = @"mutation($id: String!,$documentId: String!) {
                            createUpdateAssessmentNode(
                                id: $id, idType: INTERNAL,
                                assessmentNode:{
                                    customFields: [{id: ""HZS9LrIOUHoV"", value: ""upsert text""}],                                
                                    customRelationships: [{
                                        action: ADD,
                                        relationId: ""FXS9yxIOUf)V"",
                                        list: [{id: $documentId}]
                                    }]
                                
                         }) {
                            text: customField(id: ""HZS9LrIOUHoV"")
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") { id }
                        } }",
                Variables = new
                {
                    id = node.Id,
                    documentId = document.Id
                }
            };

            var upsertResponse = await graphQLClient.SendQueryAsync<CreateUpdateAssessmentNodeResponse>(upsertRequest);

            upsertResponse.Should().HaveNoError();
            node = upsertResponse.Data.CreateUpdateAssessmentNode;
            node.Text.Should().Be("upsert text");
            node.Docs.Select(d => d.Id).Should().HaveCount(2).And.Contain("MYS9E)IOUr6W", document.Id);
        }

        private static async Task<AssessmentNodeInstance> CreateEmptyNode(GraphQLHttpClient graphQLClient)
        {
            var createRequest = new GraphQLRequest()
            {
                Query = @"mutation {
                            createAssessmentNode(assessmentNode:{})
                        { id }}"
            };
            var createResponse = await graphQLClient.SendQueryAsync<CreateAssessmentNodeResponse>(createRequest);
            createResponse.Should().HaveNoError();
            var node = createResponse.Data.CreateAssessmentNode;
            return node;
        }

        private static async Task<AssessmentNodeInstance> CreateNodeWithADocument(GraphQLHttpClient graphQLClient)
        {
            var createRequest = new GraphQLRequest()
            {
                Query = @"mutation {
                            createAssessmentNode(assessmentNode:{
                                customRelationships: [{
                                    action: ADD,
                                    relationId: ""FXS9yxIOUf)V"",
                                    list: [{id: ""MYS9E)IOUr6W""}]
                                }]
                            })
                        {
                            id
                            docs: customRelationship(id: ""FXS9yxIOUf)V"") {id}
                        }}"
            };
            var createResponse = await graphQLClient.SendQueryAsync<CreateAssessmentNodeResponse>(createRequest);
            createResponse.Should().HaveNoError();
            var node = createResponse.Data.CreateAssessmentNode;
            node.Docs[0].Id.Should().Be("MYS9E)IOUr6W");
            return node;
        }

        private static async Task<BusinessDocument> CreateDocument(GraphQLHttpClient graphQLClient)
        {
            var documentRequest = new GraphQLRequest()
            {
                Query = @"mutation {
                            createBusinessDocument(businessDocument:{ name:""My Document""})
                            { id, name, downloadUrl, uploadUrl }
                          }"
            };
            var documentResponse = await graphQLClient.SendQueryAsync<CreateBusinessDocumentResponse>(documentRequest);
            documentResponse.Should().HaveNoError();
            var document = documentResponse.Data.CreateBusinessDocument;
            return document;
        }

        public class AssessmentNodesResponse
        {
            public List<AssessmentNodeInstance> AssessmentNode { get; set; }            
        }

        public class CreateAssessmentNodeResponse
        {
            public AssessmentNodeInstance CreateAssessmentNode { get; set; }
        }

        public class UpdateAssessmentNodeResponse
        {
            public AssessmentNodeInstance UpdateAssessmentNode { get; set; }
        }

        public class CreateUpdateAssessmentNodeResponse
        {
            public AssessmentNodeInstance CreateUpdateAssessmentNode { get; set; }
        }

        public class AssessmentNodeInstance
        {
            public string Id { get; set; }
            public string ExternalId { get; set; }
            public string EnumCombo { get; set; }
            public string Text { get; set; }
            public string BoolCheck { get; set; }
            public string DocCollection { get; set; }

            public List<DocumentInstance> Docs { get; set; }

            public class DocumentInstance
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public string CustomField { get; set; }
            }
        }
    }
}
