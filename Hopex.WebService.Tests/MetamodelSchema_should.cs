using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class MetamodelSchema_should : MockRootBasedFixture
    {
        private const string MCID_METACLASS = "P20000000c10";
        private const string MCID_METAATRIBUTE = "O20000000Y10";
        private const string MCID_ABSTRACTPROPERTY = "HQTxP4GkETZT";

        [Fact]
        public async void List_MetaAttributes_through_compiled_metamodel()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("MrUiM9B5iyM0", MCID_METACLASS))
                .WithObject(new MockMegaObject("2yUL4SsRp4B0", MCID_METAATRIBUTE))
                .WithClassDescription(new MockClassDescription("MrUiM9B5iyM0")
                    .WithMetaProperty(new MockPropertyDescription("2yUL4SsRp4B0")))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass {
                    id
                    metaAttribute {
                        id
                    }
                }}", "MetaModel");

            resp.Should().MatchGraphQL("data.metaClass[0].id", "MrUiM9B5iyM0");
            resp.Should().ContainsGraphQLCount("data.metaClass[0].metaAttribute", 1);
            resp.Should().MatchGraphQL("data.metaClass[0].metaAttribute[0].id", "2yUL4SsRp4B0");
        }

        [Fact]
        public async void List_only_MetaAttributes_and_not_other_properties()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("MrUiM9B5iyM0", MCID_METACLASS))
                .WithObject(new MockMegaObject("2yUL4SsRp4B0", MCID_ABSTRACTPROPERTY))
                .WithClassDescription(new MockClassDescription("MrUiM9B5iyM0")
                    .WithMetaProperty(new MockPropertyDescription("2yUL4SsRp4B0")))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass {
                    id
                    metaAttribute {
                        id
                    }
                }}", "MetaModel");

            resp.Should().ContainsGraphQLCount("data.metaClass[0].metaAttribute", 0);
        }

        [Fact]
        public async void List_all_submetaclasses()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~adwh93nhDHtG[Assessable Object]", MCID_METACLASS))
                .WithObject(new MockMegaObject("~KGqbmkPPGrEJ[Business Line]", MCID_METACLASS))
                .WithClassDescription(new MockClassDescription("~adwh93nhDHtG[Assessable Object]")
                    .WithRelation(new MockMegaCollection("~0fs9P5Ogg1fC[LowerClasses]")
                        .WithChildren(new MockMegaObject("~KGqbmkPPGrEJ[Business Line]"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass(filter: {id: ""adwh93nhDHtG""}) {
                    id
                    metaClass_SubMetaClass {
                        id
                    }
                }}", "MetaModel");

            resp.Should().HaveNoGraphQLError();
            resp.Should().ContainsGraphQLCount("data.metaClass[0].metaClass_SubMetaClass", 1);
            resp.Should().MatchGraphQL("data.metaClass[0].metaClass_SubMetaClass[0].id", "KGqbmkPPGrEJ");
        }

        [Fact]
        public async void List_all_supermetaclasses()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~adwh93nhDHtG[Assessable Object]", MCID_METACLASS))
                .WithObject(new MockMegaObject("~KGqbmkPPGrEJ[Business Line]", MCID_METACLASS))
                .WithClassDescription(new MockClassDescription("~KGqbmkPPGrEJ[Business Line]")
                    .WithRelation(new MockMegaCollection("~(es9P5ufg1fC[UpperClasses]")
                        .WithChildren(new MockMegaObject("~adwh93nhDHtG[Assessable Object]"))))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass(filter: {id: ""KGqbmkPPGrEJ""}) {
                    id
                    metaClass_SuperMetaClass {
                        id
                    }
                }}", "MetaModel");

            resp.Should().HaveNoGraphQLError();
            resp.Should().ContainsGraphQLCount("data.metaClass[0].metaClass_SuperMetaClass", 1);
            resp.Should().MatchGraphQL("data.metaClass[0].metaClass_SuperMetaClass[0].id", "adwh93nhDHtG");
        }

        [Theory]
        [InlineData("metaClass_SubMetaClass")]
        [InlineData("metaClass_SuperMetaClass")]
        public async void Not_support_erql_filters_on_compiled_metaclass_leg(string relationship)
        {
            var root = new MockMegaRoot.Builder().Build();

            var resp = await ExecuteQueryAsync(root, $@"query {{
                metaClass {{
                    id
                    {relationship}(filter:{{name:""toto""}}) {{ id }}
                }}}}", "MetaModel");

            resp.Should().MatchGraphQL("errors[0].message", "Unknown argument \"filter\"*");
        }

        [Theory]
        [InlineData("metaClass_SubMetaClass")]
        [InlineData("metaClass_SuperMetaClass")]
        public async void Not_support_ordering_on_compiled_metaclass_leg(string relationship)
        {
            var root = new MockMegaRoot.Builder().Build();

            var resp = await ExecuteQueryAsync(root, $@"query {{
                metaClass {{
                    id
                    {relationship}(orderBy: name_ASC)  {{ id }}
                }}}}", "MetaModel");

            resp.Should().MatchGraphQL("errors[0].message", "Unknown argument \"orderBy\"*");
        }

        [Fact]
        public async void List_schemas_containing_metaclass()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("MrUiM9B5iyM0", MCID_METACLASS))                
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass {
                    schemas {
                        name
                }}}", "MetaModel");

            resp.Should().ContainsGraphQLCountGreaterThan("data.metaClass[0].schemas", 1);
        }

        [Fact]
        public async void Do_not_list_schemas_if_metaclass_not_a_query_node()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("030000000240", MCID_METACLASS)) // ResponsibilityAssignment
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass {
                    schemas {
                        name
                }}}", "MetaModel");

            resp.Should().ContainsGraphQLCount("data.metaClass[0].schemas", 0);
        }

        [Fact]
        public async void Get_name_in_schema_for_a_given_metaclass()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("MrUiM9B5iyM0", MCID_METACLASS))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                metaClass {
                    schemas {
                        graphQLNameInSchema
                }}}", "MetaModel");

            resp.Should().HaveNoGraphQLError();
            resp.Should().ContainsGraphQLCountGreaterThan("data.metaClass[0].schemas", 1)
                .And.MatchAllGraphQL("data.metaClass[0].schemas", "graphQLNameInSchema", "application" );
        }

        [Fact]
        public async void Get_mapping_for_mettaattribute()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("MrUiM9B5iyM0", MCID_METACLASS))
                .WithObject(new MockMegaObject("2yUL4SsRp4B0", MCID_METAATRIBUTE))
                .WithClassDescription(new MockClassDescription("MrUiM9B5iyM0")
                    .WithMetaProperty(new MockPropertyDescription("2yUL4SsRp4B0")))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                 metaClass {
                    metaAttribute {
                        schemas {
                            name
                            graphQLNameInSchema
                        }
                    }
                }}", "MetaModel");

            resp.Should().ContainsGraphQLCountGreaterThan("data.metaClass[0].metaAttribute[0].schemas", 1)
                .And.MatchAllGraphQL("data.metaClass[0].metaAttribute[0].schemas", "graphQLNameInSchema", "applicationCode");
        }

        [Theory]
        [InlineData("metaClass_SubMetaClass", "~0fs9P5Ogg1fC[LowerClasses]")]
        [InlineData("metaClass_SuperMetaClass", "~(es9P5ufg1fC[UpperClasses]")]
        public async void Filter_related_metaclasses_for_a_given_schema(string relationship, string compiledLeg)
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~adwh93nhDHtG[Assessable Object]", MCID_METACLASS))
                .WithObject(new MockMegaObject("~KGqbmkPPGrEJ[Business Line]", MCID_METACLASS))
                .WithObject(new MockMegaObject("~aMRn)bUIGjX3[System Business Document]", MCID_METACLASS))
                .WithClassDescription(new MockClassDescription("~adwh93nhDHtG[Assessable Object]")
                    .WithRelation(new MockMegaCollection(compiledLeg)
                        .WithChildren(new MockMegaObject("~KGqbmkPPGrEJ[Business Line]"))
                        .WithChildren(new MockMegaObject("~aMRn)bUIGjX3[System Business Document]"))))
                .Build();
            
            var resp = await ExecuteQueryAsync(root, $@"query {{
                metaClass(first: 1) {{
                    id
                    {relationship}(fromSchema:[""ItPm""]) {{ id }}
                }}}}", "MetaModel");

            resp.Should().HaveNoGraphQLError();
            resp.Should().ContainsGraphQLCount($"data.metaClass[0].{relationship}", 1);
            resp.Should().MatchGraphQL($"data.metaClass[0].{relationship}[0].id", "KGqbmkPPGrEJ");
        }
    }
}
