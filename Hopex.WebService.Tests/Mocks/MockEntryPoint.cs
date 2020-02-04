using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL;
using Hopex.Modules.GraphQL.Schema;
using Mega.Macro.API;
using Moq;
using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockEntryPoint : EntryPoint
    {
        private readonly MockraphQLRequest _request;
        private readonly IMegaRoot _megaRoot;
        private readonly Mock<MockMegaRoot> _spyRoot = new Mock<MockMegaRoot>() { CallBase = true };

    public MockEntryPoint(string schema = "itpm", int maxCollectionSize = 4)
            :base(new TestableSchemaManagerProvider())
        {
            MockDataModel.MaxCollectionSize = maxCollectionSize;
            _request = new MockraphQLRequest(schema);
            _megaRoot = _spyRoot.Object;
            (this as IHopexWebService).SetHopexContext(_megaRoot, _request, new Logger());
        }

        public string RequestPath { get => _request.Path; }

        public IHopexDataModel DataModel { get; set; }
        
        protected override MegaRoot GetNativeMegaRoot()
        {
            return null;
        }

        public override IMegaRoot GetMegaRoot()
        {
            return _megaRoot;
        }

        protected override IHopexDataModel CreateDataModel(IHopexMetaModel metaModel)
        {
            return DataModel = MockDataModel.Create(metaModel);
        }
    }

    class TestableSchemaManagerProvider : SchemaManagerProvider
    {
        protected override void WriteLogFilenameToMegaErr(IHopexContext hopexContext) { }
    }
}
