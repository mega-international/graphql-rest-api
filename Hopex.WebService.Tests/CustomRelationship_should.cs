using FluentAssertions;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System.Collections.Generic;
using Xunit;

using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;

namespace Hopex.WebService.Tests
{
    public class CustomRelationship_should : MockRootBasedFixture
    {
        private readonly MegaId MCID_ASSESSMENT_NODE = MegaId.Create(")0Gb81(DEL3o");
        private readonly MegaId MCID_BUSINESS_DOCUMENT = MegaId.Create("UkPT)TNyFDK5");
        private readonly MegaId ID_INSTANCE = MegaId.Create("Ge08B0INU100");

        [Fact]
        public async void Query_a_custom_relation()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        __typename
                        id  
                    }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].__typename", "GraphQLObject");
            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].id", "bnFLzJbPU100");
        }

        [Fact]
        public async void Query_custom_property_of_a_custom_relation()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100")
                            .WithProperty("(YByTkohHrGG", "customNestedValue"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        customField(id: ""(YByTkohHrGG"")
                    }
                }}", "Assessment");


            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].customField", "customNestedValue");
        }

        [Fact]
        public async void Query_custom_translated_property_of_a_custom_relation()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100")
                            .WithTranslatedProperty("(YByTkohHrGG", "customNestedValue", new Dictionary<MegaId, object>()
                                {
                                    {"B0SNPuLckCQ3", "valeur personnalisée imbriquée" }
                                }))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        customField(id: ""(YByTkohHrGG"", language: FR)
                    }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].customField", "valeur personnalisée imbriquée");
        }

        [Fact]
        public async void Query_custom_relation_of_a_custom_relation()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100")
                            .WithRelation(new MockMegaCollection("(YByTkohHrGG")
                                .WithChildren(new MockMegaObject("jMah6vAQU100"))))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        customRelationship(id: ""(YByTkohHrGG"") {
                            id
                        }
                    }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].customRelationship[0].id", "jMah6vAQU100");
        }

        [Fact]
        public async void Return_empty_list_when_collection_is_not_readable()
        {
            var spyCollectionDescription = new Mock<MockCollectionDescription>("FXS9yxIOUf)V");
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100"))))
                .WithCollectionDescription(spyCollectionDescription.Object)
                .Build();
            spyCollectionDescription
                .Setup(c => c.CallFunctionString(IsMegaId("~f8pQpjMDK1SP[GetMetaPermission]"), null, null, null, null, null, null))
                .Returns("CUD");

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        id                        
                    }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.assessmentNode[0].docs", "");
        }


        [Theory]
        [InlineData("first", "2",
            "000000000001", "000000000002")]
        [InlineData("before", "\"000000000004\"",
            "000000000001", "000000000002", "000000000003")]
        [InlineData("after", "\"000000000002\"",
            "000000000003", "000000000004", "000000000005")]
        [InlineData("last", "2", "000000000004",
            "000000000005")]
        [InlineData("skip", "1", "000000000002",
            "000000000003", "000000000004", "000000000005")]
        public async void Support_pagination_control_on_custom_relation(string argument, string argumentValue, params string[] expectedIds)
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("000000000001"))
                        .WithChildren(new MockMegaObject("000000000002"))
                        .WithChildren(new MockMegaObject("000000000003"))
                        .WithChildren(new MockMegaObject("000000000004"))
                        .WithChildren(new MockMegaObject("000000000005"))
                        ))
                .Build();

            var resp = await ExecuteQueryAsync(root, $@"query {{
                assessmentNode {{
                    customRelationship(id: ""FXS9yxIOUf)V"", {argument}: {argumentValue}) {{
                        id                        
                    }}
                }}}}", "Assessment");

            resp.Should().ContainsGraphQLCount("data.assessmentNode[0].customRelationship", expectedIds.Length);
            for (var i = 0; i < expectedIds.Length; i++)
                resp.Should().MatchGraphQL($"data.assessmentNode[0].customRelationship[{i}].id", expectedIds[i]);
        }        

        [Fact]
        public async void Add_a_child_in_a_custom_relation()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")))
                .WithObject(new MockMegaObject("39cXIxu2HHrI"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{action: ADD, relationId: ""FXS9yxIOUf)V"", list:[{ id: ""39cXIxu2HHrI"" }]}]
                }) {
                    customRelationship(id: ""FXS9yxIOUf)V"") { id }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.updateAssessmentNode.customRelationship[0].id", "39cXIxu2HHrI");
            spyRoot.Verify();
        }

        [Fact]
        public async void Return_error_when_adding_child_in_custom_relation_forbidden_by_crud()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var spyNode = CreateSpyNodeWithCRUD("RUD");
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(spyNode.Object
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")))
                .WithObject(new MockMegaObject("39cXIxu2HHrI", MCID_BUSINESS_DOCUMENT))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{action: ADD, relationId: ""FXS9yxIOUf)V"", list:[{ id: ""39cXIxu2HHrI"" }]}]
                }) {
                    id
                }}", "Assessment");

            resp.Should().MatchGraphQL("errors[0].message", "*not allowed*FXS9yxIOUf)V*");
            spyNode.Verify();
        }        

        [Fact]
        public async void Remove_a_child_in_a_custom_relation()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("39cXIxu2HHrI"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{action: REMOVE, relationId: ""FXS9yxIOUf)V"", list:[{ id: ""39cXIxu2HHrI"" }]}]
                }) {
                    customRelationship(id: ""FXS9yxIOUf)V"") { id }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.updateAssessmentNode.customRelationship", "[]");
            spyRoot.Verify();
        }

        [Fact]
        public async void Return_error_when_removing_child_in_custom_relation_forbidden_by_crud()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var spyNode = CreateSpyNodeWithCRUD("CRU");
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(spyNode.Object
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("39cXIxu2HHrI", MCID_BUSINESS_DOCUMENT))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{action: REMOVE, relationId: ""FXS9yxIOUf)V"", list:[{ id: ""39cXIxu2HHrI"" }]}]
                }) {
                    id
                }}", "Assessment");

            resp.Should().MatchGraphQL("errors[0].message", "*not allowed*FXS9yxIOUf)V*");
            spyNode.Verify();
        }

        [Fact]
        public async void Replace_children_in_a_custom_relation()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("39cXIxu2HHrI"))
                        .WithChildren(new MockMegaObject("2a3twmTUU100"))))
                .WithObject(new MockMegaObject("A1UsXdTUU100"))
                .WithObject(new MockMegaObject("Ca3t3nTUU500"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{
                        action: REPLACE_ALL,
                        relationId: ""FXS9yxIOUf)V"",
                        list:[{ id: ""A1UsXdTUU100"" }, { id: ""Ca3t3nTUU500"" }]}]
                }) {
                    customRelationship(id: ""FXS9yxIOUf)V"") { id }
                }}", "Assessment");

            resp.Should().ContainsGraphQLCount("data.updateAssessmentNode.customRelationship", 2);
            resp.Should().MatchGraphQL("data.updateAssessmentNode.customRelationship[0].id", "A1UsXdTUU100");
            resp.Should().MatchGraphQL("data.updateAssessmentNode.customRelationship[1].id", "Ca3t3nTUU500");
            spyRoot.Verify();
        }

        [Fact]
        public async void Return_error_when_replacing_child_in_custom_relation_forbidden_by_crud()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var spyNode = CreateSpyNodeWithCRUD("CRU");
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(spyNode.Object
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("39cXIxu2HHrI", MCID_BUSINESS_DOCUMENT))))
                .WithObject(new MockMegaObject("A1UsXdTUU100"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateAssessmentNode(id: ""IubjeRlyFfT1"", assessmentNode: {
                    customRelationships: [{
                        action: REPLACE_ALL,
                        relationId: ""FXS9yxIOUf)V"",
                        list:[{ id: ""A1UsXdTUU100"" }]}]
                }) {
                    id
                }}", "Assessment");

            resp.Should().MatchGraphQL("errors[0].message", "*not allowed*FXS9yxIOUf)V*");
            spyNode.Verify();
        }

        [Theory]
        [InlineData("REPLACE_ALL")]
        [InlineData("ADD")]
        public async void Upsert_inexisting_object_with_a_custom_relation(string action)
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithRelation(new MockMegaCollection(MCID_ASSESSMENT_NODE))
                .WithObject(new MockMegaObject("A1UsXdTUU100"))
                .WithMetaAssociationEnd(MCID_ASSESSMENT_NODE, "FXS9yxIOUf)V")
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation($action:_InputCollectionActionEnum!) {
                createUpdateAssessmentNode(
                    id: ""IubjeRlyFfT1"",
                    idType: INTERNAL,
                    assessmentNode: {
                        customRelationships: [{
                            action: $action,
                            relationId: ""FXS9yxIOUf)V"",
                            list:[{ id: ""A1UsXdTUU100"" }]}]
                }) {
                    customRelationship(id: ""FXS9yxIOUf)V"") { id }
                }}", "Assessment", new { action });

            resp.Should().ContainsGraphQLCount("data.createUpdateAssessmentNode.customRelationship", 1);
            resp.Should().MatchGraphQL("data.createUpdateAssessmentNode.customRelationship[0].id", "A1UsXdTUU100");
            spyRoot.Verify();
        }

        [Theory]
        [InlineData("REPLACE_ALL", "A1UsXdTUU100")]
        [InlineData("ADD", "39cXIxu2HHrI", "A1UsXdTUU100")]
        public async void Upsert_existing_object_with_a_custom_relation(string action, params string[] expectedDocumentsId)
        {
            var spyRoot = CreateSpyRootWithPublish();
            var spyNode = CreateSpyNodeWithCRUD("CRUD");
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(spyNode.Object
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("39cXIxu2HHrI", MCID_BUSINESS_DOCUMENT))))
                .WithObject(new MockMegaObject("A1UsXdTUU100", MCID_BUSINESS_DOCUMENT)).Build();

            var resp = await ExecuteQueryAsync(root, @"mutation($action:_InputCollectionActionEnum!) {
                createUpdateAssessmentNode(
                    id: ""IubjeRlyFfT1"",
                    idType: INTERNAL,
                    assessmentNode: {
                        customRelationships: [{
                            action: $action,
                            relationId: ""FXS9yxIOUf)V"",
                            list:[{ id: ""A1UsXdTUU100"" }]}]
                }) {
                    id
                    customRelationship(id: ""FXS9yxIOUf)V"") { id }
                }}", "Assessment", new { action });

            resp.Should().MatchGraphQL("data.createUpdateAssessmentNode.id", "IubjeRlyFfT1");
            resp.Should().ContainsGraphQLCount("data.createUpdateAssessmentNode.customRelationship", expectedDocumentsId.Length);
            for (var i = 0; i < expectedDocumentsId.Length; i++)
                resp.Should().MatchGraphQL($"data.createUpdateAssessmentNode.customRelationship[{i}].id", expectedDocumentsId[i]);
            spyRoot.Verify();
            spyNode.Verify();
        }

        [Fact]
        public async void Support_polymorphism()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(ID_INSTANCE, MCID_ASSESSMENT_NODE)
                    .WithRelation(new MockMegaCollection("FXS9yxIOUf)V")
                        .WithChildren(new MockMegaObject("bnFLzJbPU100", MCID_BUSINESS_DOCUMENT)
                            .WithProperty("~7L73ZGK3R9SD[GDPRDocumentID]", "myGdprId"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                assessmentNode {
                    docs: customRelationship(id: ""FXS9yxIOUf)V"") {
                        __typename
                        id
                        ... on BusinessDocument {
                            gDPRDocumentID
                        }
                    }
                }}", "Assessment");

            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].__typename", "BusinessDocument");
            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].id", "bnFLzJbPU100");
            resp.Should().MatchGraphQL("data.assessmentNode[0].docs[0].gDPRDocumentID", "myGdprId");
        }

        private Mock<MockMegaObject> CreateSpyNodeWithCRUD(string crud)
        {
            var spyNode = new Mock<MockMegaObject>(MegaId.Create("IubjeRlyFfT1"), MCID_ASSESSMENT_NODE) { CallBase = true };
            spyNode.Setup(n => n.CallFunctionString(IsId("~R2mHVReGFP46[WFQuery]"), IsIdObject("FXS9yxIOUf)V"), IsIdObject(MCID_BUSINESS_DOCUMENT), null, null, null, null))
                .Returns(crud)
                .Verifiable();
            return spyNode;
        }
    }
}
