using GraphQL.Utilities;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL.Schema;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class SchemaEntryPoint : HopexWebService<object>
    {
        private const string WebServiceRoute = "schema";

        private ISchemaManagerProvider _schemaManagerProvider;

        public SchemaEntryPoint()
            : this(new SchemaManagerProvider())
        { }

        public SchemaEntryPoint(ISchemaManagerProvider schemaManagerSingleton)
        {
            _schemaManagerProvider = schemaManagerSingleton;
        }

        public async override Task<HopexResponse> Execute(object arg)
        {
            var schemaManager = _schemaManagerProvider.GetInstance(Logger, HopexContext);            

            var schemaPath = ExtractSchemaPath(GetRoot(), HopexContext.Request.Path);
            var (graphQlSchema, _) = await schemaManager.GetSchemaAsync(schemaPath);

            var printer = new SchemaPrinter(graphQlSchema);
            var response = new SchemaMacroResponse
            {
                Schema = printer.Print()
            };
            var result = HopexResponse.Json(JsonConvert.SerializeObject(response));
            return result;
        }

        protected virtual IMegaRoot GetRoot()
        {
            return RealMegaRootFactory.FromNativeRoot(HopexContext.NativeRoot);
        }

        private string ExtractSchemaPath(IMegaRoot iRoot, string requestPath)
        {
            return new SchemaPathExtractor().Extract(iRoot, requestPath, "", WebServiceRoute, Logger);
        }
    }
}
