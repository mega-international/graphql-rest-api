using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockEntryPoint : EntryPoint
    {
        private readonly HopexRequest _request;

        public MockEntryPoint(string schema = "itpm")
        {
            _request = new HopexRequest(schema);
            (this as IHopexWebService).SetHopexContext(new object(), _request, new Logger());
        }

        public string RequestPath { get => _request.Path; }

        public IHopexDataModel DataModel { get; set; }

        protected override IHopexDataModel CreateDataModel(IHopexMetaModel metaModel)
        {
            return DataModel = MockDataModel.Create(metaModel);
        }
    }
}
