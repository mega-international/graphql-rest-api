using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    public class QuestionnaireBuilder_should : BaseFixture
    {
        private GraphQLHttpClient _metamodelClient;

        public QuestionnaireBuilder_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _metamodelClient = await _fx.GetGraphQLClientAsync("Metamodel");
        }

        [Fact]
        public async Task<MetaclassNodesResponse> List_concrete_metaclasses()
        {
            var request = new GraphQLRequest()
            {
                Query = @"query metaclass($id:String!, $language:Languages!) {
                            metaClass(filter:{id:$id}) {
                                metaClass_SubMetaClass {
                                    id
                                    nameLanguage:name(language:$language)
                                    metaLayer
                                    schemas {
                                        name
                                        graphQLNameInSchema
                                    }
                                }
                            }
                        }", //metaClass_SubMetaClass(filter:{metaLayer:Concret}){
                Variables = new { id = "adwh93nhDHtG", language = "FR" } // "dmuyO(mWU1AW" : Answered element only in 900
            };

            var response = await _metamodelClient.SendQueryAsync<MetaclassNodesResponse>(request);

            response.Should().HaveNoError();
            var subClasses = response.Data.MetaClass[0].MetaClass_SubMetaClass;
            subClasses.Should().HaveCountGreaterThan(10);
            subClasses.Should().Contain(sc => sc.NameLanguage == "Acteur"); // a french name
            return response.Data;
        }

        [Fact]
        public async void Query_a_metaclass_after_introspection()
        {
            var metaclassResponse = await List_concrete_metaclasses();
            var keyInSchema = metaclassResponse.MetaClass[0].MetaClass_SubMetaClass
                .Where(subclass => subclass.MetaLayer == "Concret")
                .SelectMany(subclass => subclass.Schemas)
                .Where(schema => schema.Name == "Assessment")
                .First();
            var assessmentClient = await _fx.GetGraphQLClientAsync(keyInSchema.Name);
            var typeName = keyInSchema.GraphQLNameInSchema;

            var response = await assessmentClient.SendQueryAsync<ExpandoObject>(
                $"query {{ {typeName}{{ id name }} }}");

            response.Should().HaveNoError();
            response.Data.Should().NotBeNull();
            ((IDictionary<string, object>)response.Data).Should().ContainKey(typeName);
        }
    }
}
