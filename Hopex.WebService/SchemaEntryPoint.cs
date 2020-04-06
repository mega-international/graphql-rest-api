using GraphQL.Utilities;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model;
using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL.Schema;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class SchemaEntryPoint : HopexWebService<object>
    {
        private const string WebServiceRoute = "schema";

        private ISchemaManagerProvider _schemaManagerProvider;
        private ILanguagesProvider _languagesProvider;

        public SchemaEntryPoint()
            : this(new SchemaManagerProvider(), new LanguagesProvider())
        { }

        public SchemaEntryPoint(ISchemaManagerProvider schemaManagerSingleton, ILanguagesProvider languagesProvider)
        {
            _schemaManagerProvider = schemaManagerSingleton;
            _languagesProvider = languagesProvider;
        }

        public async override Task<HopexResponse> Execute(object arg)
        {
            var environmentId = ContextUtils.GetEnvironmentId(HopexContext);
            var schemaRef = ExtractSchemaPath(GetRoot(), HopexContext.Request.Path, environmentId);
            var schemaManager = await _schemaManagerProvider.GetInstanceAsync(Logger, HopexContext, schemaRef.Version);

            var languages = _languagesProvider.GetLanguages(HopexContext.NativeRoot);

            if (schemaRef == null)
            {
                var ex = new Exception($"Unknown route: {HopexContext.Request.Path}");
                Logger?.LogError(ex);
                if (ContextUtils.IsRunningInHAS)
                    return HopexResponse.Error(500, ex.Message);
                else
                    return HopexResponse.Error((int)HttpStatusCode.BadRequest, JsonConvert.SerializeObject(new { HttpStatusCode = HttpStatusCode.BadRequest, Error = ex.Message }));
            }
            var (graphQlSchema, _) = await schemaManager.GetSchemaAsync(schemaRef, languages);

            var printer = new SchemaPrinter(graphQlSchema);
            var response = new SchemaMacroResponse
            {
                Schema = printer.Print()
            };

            if (ContextUtils.IsRunningInHAS)
                return HopexResponse.Json(JsonConvert.SerializeObject(response)); // TODO
            else
                return HopexResponse.Json(JsonConvert.SerializeObject(response));
        }

        protected virtual IMegaRoot GetRoot()
        {
            return RealMegaRootFactory.FromNativeRoot(HopexContext.NativeRoot);
        }

        private SchemaReference ExtractSchemaPath(IMegaRoot iRoot, string requestPath, string environmentId)
        {
            return EntryPoint._schemaExtractor.Extract(iRoot, requestPath, WebServiceRoute, environmentId);
        }
    }
}
