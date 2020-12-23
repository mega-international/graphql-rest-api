using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Hopex.Model.Mocks;
using Hopex.Model.PivotSchema.Convertors;
using Hopex.Modules.GraphQL;
using Hopex.Modules.GraphQL.Schema;
using Mega.Macro.API;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public class TestableSchemaManagerProvider : SchemaManagerProvider
    {
        private bool _generateNewSchema { get; set; } = false;

        public TestableSchemaManagerProvider(bool generateNewSchema = false)
        {
            _generateNewSchema = generateNewSchema;
        }

        public override async Task<GraphQLSchemaManager> GetInstanceAsync(IHopexContext hopexContext, string version, IMegaRoot megaRoot, ILogger logger)
        {
            if (_generateNewSchema)
            {
                var hopexSchemaManager = new HopexMetaModelManager(CreateSchemaLoader(), ctx => new PivotConvertor(ctx));
                await hopexSchemaManager.LoadAllAsync(version);
                return new GraphQLSchemaManager(hopexSchemaManager, megaRoot, logger);
            }
            return await base.GetInstanceAsync(hopexContext, version, megaRoot, logger);
        }

        protected override void WriteLogFilenameToMegaErr(IHopexContext hopexContext) { }
    }

    class TestableLanguageProvider : ILanguagesProvider
    {
        public Dictionary<string, IMegaObject> GetLanguages(ILogger logger, IMegaRoot root)
        {
            var mockEnglishLanguage = new Mock<IMegaObject>();
            mockEnglishLanguage.SetupGet(x => x.Id).Returns("~00(6wlHmk400");
            var mockFrenchLanguage = new Mock<IMegaObject>();
            mockFrenchLanguage.SetupGet(x => x.Id).Returns("~B0SNPuLckCQ3");
            return new Dictionary<string, IMegaObject>
            {
                {"EN", mockEnglishLanguage.Object},
                {"FR", mockFrenchLanguage.Object}
            };
        }

        public List<string> GetCurrencies(ILogger logger, IMegaRoot root)
        {
            return new List<string> {"USD", "EUR"};
        }
    }
}
