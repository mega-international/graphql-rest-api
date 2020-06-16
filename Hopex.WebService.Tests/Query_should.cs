using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using System.Diagnostics.CodeAnalysis;
using Mega.Macro.API.Library;
using Xunit;

namespace Hopex.WebService.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Query_should : MockRootBasedFixture
    {
        private readonly MegaId _currentStatePropertyId = MegaId.Create("(YByTkohHrGG");
        private readonly MegaId _namePropertyId = MegaId.Create("Z20000000D60");

        [Fact]
        public async void Query_an_application()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString() ))
                .WithObject(new MockMegaObject(currentStateId, MetaClassLibrary.StateUml)
                    .WithProperty(MetaAttributeLibrary.ShortName, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application{id,currentState{id,name}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState.name", "Production");
        }

        [Fact]
        public async void Query_an_application_when_current_state_metaclass_does_not_exist()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");
            var fakeCurrentStateMetaclass = "itgClAZpU100";
            
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString() ))
                .WithObject(new MockMegaObject(currentStateId, fakeCurrentStateMetaclass)
                    .WithProperty(MetaAttributeLibrary.ShortName, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application{id,currentState{id,name}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState.name", "Production");
        }

        //[Fact]
        public async void Query_an_application_with_filter()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString() ))
                .WithObject(new MockMegaObject(currentStateId, MetaClassLibrary.StateUml)
                    .WithProperty(MetaAttributeLibrary.ShortName, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application(filter:{currentStateId:{name_start_with:""Prod""}}){id,currentStateId{id,name}}}");

            resp.Should().MatchGraphQL("data.application[0].currentStateId.name", "Production");
        }
    }
}
