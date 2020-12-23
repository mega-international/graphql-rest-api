using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Hopex.WebService.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SearchAll_should : MockRootBasedFixture
    {
        [Fact]
        public async void find_foo_bar_application()
        {
            var macroSearchRepositoryMock = new Mock<IMacro>();
            const string macroSearchRepositoryInput = "{\"request\":{\"Value\":\"foo\",\"Language\":\"\",\"MinRange\":0,\"MaxRange\":20,\"SortColumn\":\"Ranking\",\"SortDirection\":\"ASC\"}}";
            object macroSearchRepositoryOutput = "{\"results\":{\"SearchedString\":\"foo\",\"Language\":\"1BE01BFA2EC10001\",\"Occresults\":{\"MinRange\":0,\"OccList\":[{\"ObjectId\":\"9B6F9D725F364A3A\",\"MetaclassId\":\"B1EDB2562C14016F\",\"ObjectPath\":\"\",\"ObjectName\":\"foo bar\",\"ObjectIcon\":\"~nVv4ZMXJrm00{#type:000000000000}\",\"MetaclassName\":\"Application B1EDB2562C14016F\",\"HitCount\":5,\"Ranking\":127,\"Details\":[{\"AttributeId\":\"0000000040000063\",\"AttributeName\":\"Short Name\"},{\"AttributeId\":\"0000000040000002\",\"AttributeName\":\"Name\"}],\"FoundWords\":[{\"Word\":\"foo\"},{\"Word\":\"d1eed1f05cff1234\"}]}],\"MaxRange\":20,\"OccCount\":1},\"ParsedString\":\"(foo) AndAny (PROMOTE contains D1EED1F05CFF1234)\",\"ExhaustiveList\":1,\"FoundConfidentialResult\":1}}";
            macroSearchRepositoryMock.Setup(x => x.Generate(It.IsAny<object>(), null, macroSearchRepositoryInput, out macroSearchRepositoryOutput));

            var currentEnvironmentMock = new Mock<MockCurrentEnvironment> {CallBase = true};
            currentEnvironmentMock.Setup(x => x.GetMacro("~w9D5uK4iI9n0[SearchRepository.GetResult]")).Returns(macroSearchRepositoryMock.Object);

            var spyRoot = new Mock<MockMegaRoot> {CallBase = true};
            spyRoot.SetupGet(x => x.CurrentEnvironment).Returns(currentEnvironmentMock.Object);
            spyRoot.Setup(x => x.ConditionEvaluate("~PuC7Fh2WKv1H[Is Full Text Search Activated]")).Returns(true);

            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject(MegaId.Create("oyscorfDVfZI"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName, "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{searchAll(filter:{text:""foo"" minRange:0 maxRange:20} orderBy:ranking_ASC){id, name}}", schemaManagerProvider: new TestableSchemaManagerProvider(true));
            result.Should().MatchGraphQL("data.searchAll[0].name", "foo bar");
        }

        [Fact]
        public async void return_error_if_base_is_not_indexed()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("oyscorfDVfZI"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName, "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{searchAll(filter:{text:""foo""}){id, name}}");
            result.Should().MatchGraphQL("errors.[0].message", "Cannot query field \"searchAll\" on type \"Query\".");
        }

        [Fact]
        public async void syntax_on_application_should_work()
        {
            var macroSearchRepositoryMock = new Mock<IMacro>();
            const string macroSearchRepositoryInput = "{\"request\":{\"Value\":\"foo\",\"Language\":\"\",\"MinRange\":0,\"MaxRange\":1000,\"SortColumn\":\"Ranking\",\"SortDirection\":\"ASC\"}}";
            object macroSearchRepositoryOutput = "{\"results\":{\"SearchedString\":\"foo\",\"Language\":\"1BE01BFA2EC10001\",\"Occresults\":{\"MinRange\":0,\"OccList\":[{\"ObjectId\":\"9B6F9D725F364A3A\",\"MetaclassId\":\"B1EDB2562C14016F\",\"ObjectPath\":\"\",\"ObjectName\":\"foo bar\",\"ObjectIcon\":\"~nVv4ZMXJrm00{#type:000000000000}\",\"MetaclassName\":\"Application B1EDB2562C14016F\",\"HitCount\":5,\"Ranking\":127,\"Details\":[{\"AttributeId\":\"0000000040000063\",\"AttributeName\":\"Short Name\"},{\"AttributeId\":\"0000000040000002\",\"AttributeName\":\"Name\"}],\"FoundWords\":[{\"Word\":\"foo\"},{\"Word\":\"d1eed1f05cff1234\"}]}],\"MaxRange\":1000,\"OccCount\":1},\"ParsedString\":\"(foo) AndAny (PROMOTE contains D1EED1F05CFF1234)\",\"ExhaustiveList\":1,\"FoundConfidentialResult\":1}}";
            macroSearchRepositoryMock.Setup(x => x.Generate(It.IsAny<object>(), null, macroSearchRepositoryInput, out macroSearchRepositoryOutput));

            var currentEnvironmentMock = new Mock<MockCurrentEnvironment> { CallBase = true };
            currentEnvironmentMock.Setup(x => x.GetMacro("~w9D5uK4iI9n0[SearchRepository.GetResult]")).Returns(macroSearchRepositoryMock.Object);

            var spyRoot = new Mock<MockMegaRoot> { CallBase = true };
            spyRoot.SetupGet(x => x.CurrentEnvironment).Returns(currentEnvironmentMock.Object);
            spyRoot.Setup(x => x.ConditionEvaluate("~PuC7Fh2WKv1H[Is Full Text Search Activated]")).Returns(true);

            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject(MegaId.Create("oyscorfDVfZI"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName, "foo bar").
                    WithProperty(MetaAttributeLibrary.ApplicationCode, "foo bar code"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{searchAll(filter:{text:""foo""}){id name ...on Application{applicationCode}}}", schemaManagerProvider: new TestableSchemaManagerProvider(true));
            result.Should().MatchGraphQL("data.searchAll[0].applicationCode", "foo bar code");
        }


        [Fact]
        public async void Occurence_not_allowed_for_reading_should_not_be_returned()
        {
            var macroSearchRepositoryMock = new Mock<IMacro>();
            const string macroSearchRepositoryInput = "{\"request\":{\"Value\":\"foo\",\"Language\":\"\",\"MinRange\":0,\"MaxRange\":1000,\"SortColumn\":\"Ranking\",\"SortDirection\":\"ASC\"}}";
            object macroSearchRepositoryOutput = "{\"results\":{\"SearchedString\":\"foo\",\"Language\":\"1BE01BFA2EC10001\",\"Occresults\":{\"MinRange\":0,\"OccList\":[{\"ObjectId\":\"9B6F9D725F364A3A\",\"MetaclassId\":\"B1EDB2562C14016F\",\"ObjectPath\":\"\",\"ObjectName\":\"foo bar\",\"ObjectIcon\":\"~nVv4ZMXJrm00{#type:000000000000}\",\"MetaclassName\":\"Application B1EDB2562C14016F\",\"HitCount\":5,\"Ranking\":127,\"Details\":[{\"AttributeId\":\"0000000040000063\",\"AttributeName\":\"Short Name\"},{\"AttributeId\":\"0000000040000002\",\"AttributeName\":\"Name\"}],\"FoundWords\":[{\"Word\":\"foo\"},{\"Word\":\"d1eed1f05cff1234\"}]}],\"MaxRange\":1000,\"OccCount\":1},\"ParsedString\":\"(foo) AndAny (PROMOTE contains D1EED1F05CFF1234)\",\"ExhaustiveList\":1,\"FoundConfidentialResult\":1}}";
            macroSearchRepositoryMock.Setup(x => x.Generate(It.IsAny<object>(), null, macroSearchRepositoryInput, out macroSearchRepositoryOutput));

            var currentEnvironmentMock = new Mock<MockCurrentEnvironment> { CallBase = true };
            currentEnvironmentMock.Setup(x => x.GetMacro("~w9D5uK4iI9n0[SearchRepository.GetResult]")).Returns(macroSearchRepositoryMock.Object);

            var spyRoot = new Mock<MockMegaRoot> { CallBase = true };
            spyRoot.SetupGet(x => x.CurrentEnvironment).Returns(currentEnvironmentMock.Object);
            spyRoot.Setup(x => x.ConditionEvaluate("~PuC7Fh2WKv1H[Is Full Text Search Activated]")).Returns(true);

            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject((MockMegaObject)new MockMegaObject(MegaId.Create("oyscorfDVfZI"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName, "foo bar").
                        WithObjectCrud(""))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{searchAll(filter:{text:""foo""}){id name}}", schemaManagerProvider: new TestableSchemaManagerProvider(true));
            result.Should().ContainsGraphQLCount("data.searchAll", 0);
        }

        public interface IMacro
        {
            void Generate(dynamic root, dynamic ctx, string input, out object result);
        }
    }
}
