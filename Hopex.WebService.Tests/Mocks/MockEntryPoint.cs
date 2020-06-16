using System.Collections.Generic;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL;
using Hopex.Modules.GraphQL.Schema;
using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockEntryPoint : EntryPoint
    {
        private readonly MockraphQLRequest _request;
        private readonly IMegaRoot _megaRoot = new MockMegaRoot();
        
        public MockEntryPoint(string schema = "itpm", int maxCollectionSize = 4)
                :base(new TestableSchemaManagerProvider(), new TestableLanguageProvider())
        {
            MockDataModel.MaxCollectionSize = maxCollectionSize;
            _request = new MockraphQLRequest(schema);
            (this as IHopexWebService).SetHopexContext(_megaRoot, _request, new Logger());
        }

        public string RequestPath { get => _request.Path; }

        public MockDataModel DataModel { get; set; }

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

    class TestableLanguageProvider : ILanguagesProvider
    {
        public Dictionary<string, string> GetLanguages(object nativeRoot)
        {
            return new Dictionary<string, string>
            {
                {"EN", "~00(6wlHmk400[X]"},
                {"FR", "~B0SNPuLckCQ3[X]"}
            };
        }
    }
}
