using FluentAssertions;
using Hopex.Model.Abstractions;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class CollectionSetter_should : MockRootBasedFixture
    {
        private const string MCID_APPLICATION = "MrUiM9B5iyM0";
        private const string MCID_BUSINESS_PROCESS = "pj)grmQ9pG90";
        private const string MAEID_APPLICATION_BUSINESS_PROCESSS = "h4n)MzlZpK00";
        private const string MAID_ORDER = "~410000000H00";
        private const string MAID_NAME = "~Z20000000D60";
        private const string MAID_EXTERNAL_ID = "~CFmhlMxNT1iE";

        private readonly MockMegaObject process;
        private readonly MockMegaCollection processesOfApplication;
        private readonly MockMegaRoot.Builder rootBuilderWithApplication;

        public CollectionSetter_should()
        {
            process = new MockMegaObject("~3h2QHZMcU500[Child process]", MCID_BUSINESS_PROCESS);
            processesOfApplication = new MockMegaCollection(MAEID_APPLICATION_BUSINESS_PROCESSS);
            rootBuilderWithApplication = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~Se2QhWMcU100[Parent application]", MCID_APPLICATION)
                    .WithRelation(processesOfApplication));
        }

        [Fact]
        public async void Create_a_relation_and_set_a_link_attribute()
        {
            var root = rootBuilderWithApplication
                .WithObject(process)
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: ADD
                        list:[{id:""3h2QHZMcU500"" order:50}]
                    }
                }) {
                   id
                }}", "ITPM");

            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            process.GetPropertyValue<int>(MAID_ORDER).Should().Be(50);
            processesOfApplication.Should().BeEquivalentTo(process);
        }

        [Fact]
        public async void Set_a_link_attribute_on_an_existing_relation()
        {
            process.WithProperty(MAID_ORDER, 9999);
            processesOfApplication.WithChildren(process);
            var root = rootBuilderWithApplication.Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: ADD
                        list:[{id:""3h2QHZMcU500"" order:50}]
                    }
                }) {
                   id
                }}", "ITPM");

            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            process.GetPropertyValue<int>(MAID_ORDER).Should().Be(50);
            processesOfApplication.Should().BeEquivalentTo(process);
        }

        [Fact]
        public async void Remove_a_child()
        {
            var processRemaining = new MockMegaObject("dXZVxuNcU500", MCID_BUSINESS_PROCESS);
            processesOfApplication
                .WithChildren(process)
                .WithChildren(processRemaining);
            var root = rootBuilderWithApplication.Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: REMOVE
                        list:[{id:""3h2QHZMcU500""}]
                    }
                }) {
                   id
                }}", "ITPM");

            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            processesOfApplication.Should().BeEquivalentTo(processRemaining);
        }

        [Fact]
        public async void Replace_children()
        {
            var processToAdd = new MockMegaObject("dXZVxuNcU500", MCID_BUSINESS_PROCESS);
            processesOfApplication.WithChildren(process);
            var root = rootBuilderWithApplication
                .WithObject(processToAdd)
                .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: REPLACE_ALL
                        list:[{id:""dXZVxuNcU500""}]
                    }
                }) {
                   id
                }}", "ITPM");

            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            processesOfApplication.Should().BeEquivalentTo(processToAdd);
        }


        [Theory]
        [InlineData("RAW")]
        [InlineData("BUSINESS")]
        public async void Create_new_instance_in_relationship_and_set_class_attributes(string creationMode)
        {
            var businessProcesses = new Mock<MockMegaCollection>(MegaId.Create(MAEID_APPLICATION_BUSINESS_PROCESSS)) { CallBase = true };
            var wizard = new Mock<MockMegaWizardContext>(MockBehavior.Strict, businessProcesses.Object, "~3h2QHZMcU500[Child process]");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~Se2QhWMcU100[Parent application]", MCID_APPLICATION)
                    .WithRelation(businessProcesses.Object))
                .Build();

            businessProcesses.Setup(c => c.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]", null, null, null, null, null, null)).Returns(wizard.Object);

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: ADD
                        list:[{name:""createdProcess"" creationMode:" + creationMode + @"}]
                    }
                }) {
                        id
                        businessProcess {
                            name
                        }
                }}", "ITPM");

            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            resp.Should().ContainsGraphQLCount("data.updateApplication.businessProcess", 1);
            resp.Should().MatchGraphQL("data.updateApplication.businessProcess[0].name", "createdProcess");
        }

        [Fact]
        public async void Create_new_instance_in_relationship_when_inexisting_external_id()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("~Se2QhWMcU100[Parent application]", MCID_APPLICATION)
                    .WithRelation(processesOfApplication)
                        .WithProperty(MAID_EXTERNAL_ID, Convert.ToDouble(0)))
                            .Build();

            var resp = await ExecuteQueryAsync(root, @"mutation {
                updateApplication(id: ""Se2QhWMcU100"" application: {                                
                    businessProcess: {
                        action: ADD
                        list:[{name:""createdProcess"" idType: EXTERNAL id: ""~3h2QHZMcU500[Child process]""}]
                    }
                }) {
                    id
                    businessProcess
                    {
                        externalId
                    }
                }}", "ITPM");

            resp.Should().HaveNoGraphQLError();
            resp.Should().MatchGraphQL("data.updateApplication.id", "Se2QhWMcU100");
            resp.Should().ContainsGraphQLCount("data.updateApplication.businessProcess", 1);
            resp.Should().MatchGraphQL("data.updateApplication.businessProcess[0].externalId", "~3h2QHZMcU500[Child process]");
        }
    }
}
