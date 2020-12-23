using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [ImportMgr("DataIdCard_should.mgr")]
    public class DataIdCard_should : BaseFixture
    {
        public DataIdCard_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        [Fact(Skip = "Some tests needed")]
        public async Task Get_Terms_Starting_By_A_Ordered_By_Name_In_Ascending_Order()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Data");
            var request = new GraphQLRequest()
            {
                Query =
                @"query {
                    term (orderBy: [ name_ASC ], filter: {name_starts_with:""a""}) {
                        id
                        name
                        language {
                            id
                            name
                        }
                        concept_IdentifiedDictionaryType{
                            id
                            name
                            definitionText(format: RAW)
                        }
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<TermResponse>(request);
            response.Should().HaveNoError();

            var terms = response.Data.Term;
            terms.Should().HaveCount(2);
            terms.Should().BeInAscendingOrder(term => term.Name, StringComparer.OrdinalIgnoreCase);
            terms.Should().BeEquivalentTo(new List<Term>
            {
                new Term
                {
                    Id = "5r9025GKVTeB",
                    Name = "ATerm",
                    Language = new List<BasicObject>
                    {
                        new BasicObject
                        {
                            Id = "B0SNPuLckCQ3",
                            Name = "Français"
                        }
                    },
                    Concept_IdentifiedDictionaryType = new List<Concept_IdentifiedDictionaryType>
                    {
                        new Concept_IdentifiedDictionaryType
                        {
                            Id = "4t90JaGKVnoB",
                            Name = "Concept1",
                            DefinitionText = "My definition"
                        }
                    }
                },
                new Term
                {
                    Id = "Vt90q5GKVzkB",
                    Name = "ABTerm",
                    Language = new List<BasicObject>
                    {
                        new BasicObject
                        {
                            Id = "LXkm2GSbou10",
                            Name = "Italiano"
                        }
                    },
                    Concept_IdentifiedDictionaryType = new List<Concept_IdentifiedDictionaryType>{}
                }
            });
        }

        [Fact(Skip = "Some tests needed")]
        public async Task Get_Specific_Concept_With_Several_Fields()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Data");
            var request = new GraphQLRequest()
            {
                Query =
                @"query {
                    concept (filter:{id:""""} ) {
                        id
                        name
                        definitionText(format: RAW)
                        dataDesigner_PersonSystem {
                            id
                            name
                            email
                        }
                        businessDictionary_OwnerBusinessDictionary {
                            id
                            name
                            comment
                        }
                        term_Synonym {
                            id
                            name
                        }
                        businessInformationArea_OwnerBusinessArea_MemorizedBusinessInformation_MemorizationOfMemorizedBusinessInformation {
                            id
                            name
                            comment
                        }
                        class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization {
                            id
                            name
                            comment
                            applicationDataArea {
                                id
                                name
                                comment
                                application {
                                    id
                                    name
                                    comment
                                }
                            }
                        }
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<ConceptResponse>(request);
            response.Should().HaveNoError();
            var concepts = response.Data.Concept;
            concepts.Should().HaveCount(1);
            var concept = concepts[0];
            concept.Should().BeEquivalentTo(new List<Concept>
            {
                new Concept
                {
                    Id = "Nt9065HKVD0C",
                    Name = "ConceptDemo",
                    DefinitionText = "Definition demo",
                    DataDesigner_PersonSystem = new List<DataDesigner_PersonSystem>
                    {
                        new DataDesigner_PersonSystem
                        {
                            Id = "zTcUyq7ETbI9",
                            Name = "VAURY Jean-Marie",
                            Email = "jmvaury@mega.com"
                        }
                    },
                    BusinessDictionary_OwnerBusinessDictionary = new List<BasicObject>
                    {
                        new BasicObject
                        {
                            Id = "mq90c4HKVrtB",
                            Name = "BusinessDictionaryDemo",
                            Comment = "Comment Business Dictionary"
                        }
                    },
                    Term_Synonym = new List<BasicObject>
                    {
                        new BasicObject
                        {
                            Id = "A7rBd8ZKVn25",
                            Name = "TermSynonymDemo",
                        }
                    },
                    BusinessInformationArea_OwnerBusinessArea_MemorizedBusinessInformation_MemorizationOfMemorizedBusinessInformation = new List<BasicObject>
                    {
                        new BasicObject
                        {
                            Id = "(6rB6KZKVnZ5",
                            Name = "BusinessInformationAreaDemo",
                            Comment = "My Business Information Area Demo"
                        }
                    },
                    Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization = new List<Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization>
                    {
                        new Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization
                        {
                            Id = "5TdDBgZKVvED",
                            Name = "ClassDemo",
                            Comment = "",
                            ApplicationDataArea = new List<ApplicationDataArea>
                            {
                                new ApplicationDataArea
                                {
                                    Id = "8SdDAkZKV1RD",
                                    Name = "ApplicationDataAreaDemo",
                                    Comment = "",
                                    Application = new List<BasicObject>
                                    {
                                        new BasicObject
                                        {
                                            Id = "tzFFurZKVHcS",
                                            Name = "ApplicationDemo",
                                            Comment = ""
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact(Skip = "Some tests needed")]
        public async Task Get_Specific_Classes_By_Id_Array_With_Several_Fields()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("Data");
            var request = new GraphQLRequest()
            {
                Query =
                @"query {
                    class (filter: {id_in:[""id1"", ""id2""]}) {
                        id
                        name 
                        applicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent {
                            id
                            name
                            comment
                            applicationLocalDataStore_ApplicationDataStore {
                                id
                                name
                                comment
                            }
                        }
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<ClassResponse>(request);
            response.Should().HaveNoError();
            var classes = response.Data.Class;
            classes.Should().BeEquivalentTo(new List<Class>
                {
                    new Class
                    {
                        Id = "DgdFL2aKV5lN",
                        Name = "Class1",
                        ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent = new List<ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent>
                        {
                            new ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent
                            {
                                Id = "",
                                Name = "",
                                Comment = "",
                                ApplicationLocalDataStore_ApplicationDataStore = new List<BasicObject>
                                {
                                    new BasicObject
                                    {
                                        Id = "",
                                        Name = "",
                                        Comment = ""
                                    }
                                }
                            }
                        }
                    },
                    new Class
                    {
                        Id = ")fdFU2aKVXDO",
                        Name = "Class2",
                        ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent = new List<ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent>()
                    }
                }
            );
        }

        private class TermResponse
        {
            public List<Term> Term { get; set; }
        }

        private class ConceptResponse
        {
            public List<Concept> Concept { get; set; }
        }

        private class ClassResponse
        {
            public List<Class> Class { get; set; }
        }

        private class Class : BasicObject
        {
            public List<ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent> ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent { get; set; }
        }

        private class ApplicationDataArea_LogicalDataStoreStructure_LogicalDataAreaComponent_LogicalDataStoreComponent : BasicObject
        {
            public List<BasicObject> ApplicationLocalDataStore_ApplicationDataStore { get; set; }
        }

        private class Term : BasicObject
        {
            public List<BasicObject> Language { get; set; }
            public List<Concept_IdentifiedDictionaryType> Concept_IdentifiedDictionaryType { get; set; }
        }

        private class Concept : BasicObject
        {
            public string DefinitionText { get; set; }
            public List<DataDesigner_PersonSystem> DataDesigner_PersonSystem { get; set; }
            public List<BasicObject> BusinessDictionary_OwnerBusinessDictionary { get; set; }
            public List<BasicObject> Term_Synonym { get; set; }
            public List<BasicObject> BusinessInformationArea_OwnerBusinessArea_MemorizedBusinessInformation_MemorizationOfMemorizedBusinessInformation { get; set; }
            public List<Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization> Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization { get; set; }
        }

        private class DataDesigner_PersonSystem : BasicObject
        {
            public string Email { get; set; }
        }

        private class Class_BusinessInformationRealizer_RealizationOfBusinessInformation_BusinessInformationRealization : BasicObject
        {
            public List<ApplicationDataArea> ApplicationDataArea { get; set; }
        }

        private class ApplicationDataArea : BasicObject
        {
            public List<BasicObject> Application { get; set; }
        }

        private class Concept_IdentifiedDictionaryType : BasicObject
        {
            public string DefinitionText { get; set; }
        }
    }
}
