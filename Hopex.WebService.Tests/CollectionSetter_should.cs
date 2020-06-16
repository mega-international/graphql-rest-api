using FluentAssertions;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class CollectionSetter_should : MockRootBasedFixture
    {
        private const string MCID_APPLICATION = "MrUiM9B5iyM0";
        private const string MCID_BUSINESS_PROCESS = "pj)grmQ9pG90";
        private const string MAEID_APPLICATION_BUSINESS_PROCESSS = "h4n)MzlZpK00";
        private const string MAID_ORDER = "~410000000H00";

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
    }
}
