using FluentAssertions;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common;
using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;

namespace Hopex.WebService.Tests
{
    public class CustomField_should : CustomFieldsCommon
    {
        private readonly MegaId ApplicationMetaclassId = MegaId.Create("MrUiM9B5iyM0");
        private readonly MegaId InstanceId = MegaId.Create("Ge08B0INU100");

        [Fact]
        public async void Query_a_custom_property()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(InstanceId, ApplicationMetaclassId)
                    .WithProperty("(YByTkohHrGG", "customValue"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                application {
                    id
                    currentState:customField(id: ""(YByTkohHrGG"")
                }}");

            resp.Should().MatchGraphQL("data.application[0].currentState", "customValue");
        }

        [Fact]
        public async void Query_a_translated_custom_property()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(InstanceId, ApplicationMetaclassId)
                    .WithTranslatedProperty("(YByTkohHrGG", "customValue", new Dictionary<MegaId, object>()
                    {
                        {"B0SNPuLckCQ3", "valeur personnalisée" }
                    }))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                application {
                    id
                    currentState:customField(id: ""(YByTkohHrGG"", language: FR)
                }}");

            resp.Should().MatchGraphQL("data.application[0].currentState", "valeur personnalisée");
        }

        [Fact]
        public async void Fail_if_no_id_given()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(InstanceId, ApplicationMetaclassId)
                    .WithProperty("(YByTkohHrGG", "customValue"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query {
                application {
                    id
                    currentState:customField
                }}");

            resp.Should().MatchGraphQL("errors[0].message", "*argument \"id\" * is required*");
        }

        [Theory]
        [InlineData("Internal", "Internal")]
        [InlineData("ASCII", "ASCII")]
        [InlineData("External", "External")]
        [InlineData("Display", "Display")]
        [InlineData("Object", "Object")]
        [InlineData("Physical", "Physical")]
        public async void Query_a_custom_property_with_format(string graphQLFormat, string expectedMegaFormat)
        {
            var spyMegaObject = new Mock<MockMegaObject>(InstanceId, ApplicationMetaclassId) { CallBase = true };
            var root = new MockMegaRoot.Builder()
                .WithObject(spyMegaObject.Object)
                .Build();
            spyMegaObject.Setup(o => o.GetPropertyValue<string>("~(YByTkohHrGG", expectedMegaFormat)).Returns("customValue").Verifiable();

            var resp = await ExecuteQueryAsync(root, $@"query {{
                application {{
                    id
                    currentState:customField(id: ""(YByTkohHrGG"", format: {graphQLFormat})
                }}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState", "customValue");
            spyMegaObject.Verify();
        }        

        [Fact]
        public async void Update_the_value_of_a_custom_field()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", ApplicationMetaclassId)
                    .WithProperty("(YByTkohHrGG", "customValue"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""IubjeRlyFfT1"", application:{                    
                    customFields: [{ id: ""(YByTkohHrGG"", value: ""newCustomValue""}]
                })
                { customField(id: ""(YByTkohHrGG"") } }", "ITPM");
            
            resp.Should().MatchGraphQL("data.updateApplication.customField", "newCustomValue");
            spyRoot.Verify();
        }

        [Fact]
        public async void Update_the_value_of_multiple_custom_fields()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", ApplicationMetaclassId)
                    .WithProperty("(YByTkohHrGG", "customValue1")
                    .WithProperty("XzOEGcZSU100", "customValue2"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""IubjeRlyFfT1"", application:{                    
                    customFields: [{ id: ""(YByTkohHrGG"", value: ""newCustomValue1""}, { id: ""XzOEGcZSU100"", value: ""newCustomValue2""}]
                })
                {
                    field1: customField(id: ""(YByTkohHrGG"")
                    field2: customField(id: ""XzOEGcZSU100"")
                } }", "ITPM");

            resp.Should().MatchGraphQL("data.updateApplication.field1", "newCustomValue1");
            resp.Should().MatchGraphQL("data.updateApplication.field2", "newCustomValue2");
            spyRoot.Verify();
        }

        [Fact]
        public async void Create_an_object_with_custom_fields()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithRelation(new MockMegaCollection(ApplicationMetaclassId))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                createApplication( application: {
                    customFields: [{ id: ""(YByTkohHrGG"", value: ""newCustomValue1""}, { id: ""XzOEGcZSU100"", value: ""newCustomValue2""}]
                })
                {
                    field1: customField(id: ""(YByTkohHrGG"")
                    field2: customField(id: ""XzOEGcZSU100"")
                } }", "ITPM");

            resp.Should().MatchGraphQL("data.createApplication.field1", "newCustomValue1");
            resp.Should().MatchGraphQL("data.createApplication.field2", "newCustomValue2");
            spyRoot.Verify();
        }

        [Fact]
        public async void Upsert_an_inexisting_object_with_custom_field()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithRelation(new MockMegaCollection(ApplicationMetaclassId))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                createUpdateApplication(
                    id: ""ATTOO7sUU100"", 
                    idType: INTERNAL,
                    application: {
                    customFields: [{ id: ""(YByTkohHrGG"", value: ""newCustomValue""}]
                })
                {
                    customField(id: ""(YByTkohHrGG"")
                } }", "ITPM");

            resp.Should().MatchGraphQL("data.createUpdateApplication.customField", "newCustomValue");
            spyRoot.Verify();
        }

        [Fact]
        public async void Upsert_an_existing_object_with_custom_field()
        {
            var spyRoot = CreateSpyRootWithPublish();
            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject("IubjeRlyFfT1", ApplicationMetaclassId)
                    .WithProperty("(YByTkohHrGG", "customValue"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                createUpdateApplication(
                    id: ""IubjeRlyFfT1"", 
                    idType: INTERNAL,
                    application: {
                    customFields: [{ id: ""(YByTkohHrGG"", value: ""newCustomValue""}]
                })
                {
                    id
                    customField(id: ""(YByTkohHrGG"")
                } }", "ITPM");

            resp.Should().MatchGraphQL("data.createUpdateApplication.id", "IubjeRlyFfT1");
            resp.Should().MatchGraphQL("data.createUpdateApplication.customField", "newCustomValue");
            spyRoot.Verify();
        }
    }

    public class CustomFieldsCommon
    {

        protected TestableEntryPoint ws;

        protected static Mock<MockMegaRoot> CreateSpyRootWithPublish()
        {
            var spyRoot = new Mock<MockMegaRoot>() { CallBase = true };
            spyRoot
                .Setup(r => r.CallFunctionString(IsId("~lcE6jbH9G5cK[PublishStayInSessionWizard Command Launcher]"), "{\"instruction\":\"POSTPUBLISHINSESSION\"}", null, null, null, null, null))
                .Returns("SESSION_PUBLISH")
                .Verifiable();
            return spyRoot;
        }

        protected async Task<HopexResponse> ExecuteQueryAsync(IMegaRoot root, string query, string schema = "ITPM", object variables = null)
        {
            ws = new TestableEntryPoint(root, schema);
            var args = new InputArguments
            {
                Query = query
            };
            args.Variables = ConvertAnonymousToDictionary(variables);
            return await ws.Execute(args);
        }

        private static Dictionary<string, object> ConvertAnonymousToDictionary(object values)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj = propertyDescriptor.GetValue(values);
                    dict.Add(propertyDescriptor.Name, obj);
                }
            }
            return dict;
        }

        protected class TestableEntryPoint : EntryPoint
        {
            private readonly MockraphQLRequest _request;
            private readonly IMegaRoot _megaRoot;

            public TestableEntryPoint(IMegaRoot root, string schema)
                    : base(new TestableSchemaManagerProvider(), new TestableLanguageProvider())
            {
                _request = new MockraphQLRequest(schema);
                _megaRoot = root;
                (this as IHopexWebService).SetHopexContext(_megaRoot, _request, new Logger());
            }

            protected override MegaRoot GetNativeMegaRoot()
            {
                return null;
            }

            public override IMegaRoot GetMegaRoot()
            {
                return _megaRoot;
            }
        }
    }

}
