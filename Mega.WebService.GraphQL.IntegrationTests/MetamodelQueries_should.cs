using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    public class MetamodelQueries_should : BaseFixture
    {
        private const string MDID_LOCKABLE_OBJECT = "Ggq9LIpnFLo6";
        private const string MCID_APPLICATION = "MrUiM9B5iyM0";
        private const string MCID_GENERIC_OBJECT = "p20000000E30";

        public MetamodelQueries_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        [Fact]
        public async void List_inherited_metaatributes()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("MetaModel");
            var request = new GraphQLRequest()
            {
                Query = @"query($id: String!) {
                            metaClass(filter: {id: $id }) {
                                id
                                metaAttribute {
                                    id
                                }
                            }
                          }",
                Variables = new { id = MCID_APPLICATION }
            };

            var response = await graphQLClient.SendQueryAsync<MetaclassNodesResponse>(request);

            response.Should().HaveNoError();
            var metaattributes = response.Data.MetaClass[0].MetaAttribute;
            metaattributes.Should().Contain(a => a.Id == "L30000000L90");
            metaattributes.Should().HaveCountGreaterThan(100);
        }

        [Fact]
        public async void Query_compiled_inheritance_hierarchy()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("MetaModel");
            var request = new GraphQLRequest()
            {
                Query = @"query($id: String!) {
                            metaClass(filter: {id: $id }) {
                                id
                                metaClass_SubMetaClass {
                                    id
                                }
                                metaClass_SuperMetaClass {
                                    id
                                }
                                filteredSubMetaClass: metaClass_SubMetaClass(fromSchema:[""ITPM""]) { id }
                                filteredSuperMetaClass: metaClass_SuperMetaClass(fromSchema:[""ITPM"",""Aduit""]) { id }
                            }
                          }",
                Variables = new { id = MDID_LOCKABLE_OBJECT }
            };

            var response = await graphQLClient.SendQueryAsync<MetaclassNodesResponse>(request);

            response.Should().HaveNoError();
            var metaclassNode = response.Data.MetaClass[0];
            metaclassNode.MetaClass_SubMetaClass.Should().HaveCountGreaterThan(100);
            metaclassNode.MetaClass_SuperMetaClass.Should().HaveCountGreaterThan(0);
            metaclassNode.MetaClass_SuperMetaClass.Should().ContainSingle(c => c.Id == MCID_GENERIC_OBJECT);
            metaclassNode.FilteredSubMetaClass.Should().HaveCountLessThan(metaclassNode.MetaClass_SubMetaClass.Count);
            metaclassNode.FilteredSuperMetaClass.Should().HaveCountLessThan(metaclassNode.MetaClass_SuperMetaClass.Count);
        }       

        [Fact]
        public async void List_schema_containing_a_metaclass()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("MetaModel");
            var request = new GraphQLRequest()
            {
                Query = @"query($id: String!) {
                            metaClass(filter: {id: $id }) {
                                name
                                schemas {
                                  name
                                   graphQLNameInSchema      
                                }
                            }
                          }",
                Variables = new { id = MCID_APPLICATION }
            };

            var response = await graphQLClient.SendQueryAsync<MetaclassNodesResponse>(request);

            response.Should().HaveNoError();
            var metaclass = response.Data.MetaClass[0];
            var schemas = metaclass.Schemas;
            schemas.Should().HaveCountGreaterThan(1)
                .And.OnlyContain(s => s.GraphQLNameInSchema == ToCamelCase(metaclass.Name));
        }

        [Fact]
        public async void List_schema_containing_a_metaattribute()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("MetaModel");
            var request = new GraphQLRequest()
            {
                Query = @"query($id: String!) {
                            metaClass(filter: {id: $id }) {
                                metaAttribute {
                                    schemas {
                                        name
                                        graphQLNameInSchema      
                                    }
                                }
                            }
                          }",
                Variables = new { id = MCID_APPLICATION }
            };

            var response = await graphQLClient.SendQueryAsync<MetaclassNodesResponse>(request);

            response.Should().HaveNoError();
            var mappedAttributes = response.Data.MetaClass[0].MetaAttribute.Where(a => a.Schemas.Count > 0);
            mappedAttributes.Should().HaveCountGreaterThan(10);
            mappedAttributes.SelectMany(a => a.Schemas).Should().OnlyContain(s => !string.IsNullOrEmpty(s.Name))
                .And.OnlyContain(s => !string.IsNullOrEmpty(s.GraphQLNameInSchema));
        }

        private string ToCamelCase(string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }                
    }
}
