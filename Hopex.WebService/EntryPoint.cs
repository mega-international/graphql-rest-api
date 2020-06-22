using GraphQL;
using GraphQL.Http;
using GraphQL.Language.AST;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common;
using Hopex.Model;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.PivotSchema.Convertors;
using Hopex.Modules.GraphQL.Schema;

using Mega.Macro.API;

using Newtonsoft.Json;

using System;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    /// <summary>
    /// Web service entry point 
    /// </summary>
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class EntryPoint : HopexWebService<InputArguments>
    {
        private const string WebServiceRoute = "graphql";
        internal static SchemaPathExtractor _schemaExtractor = new SchemaPathExtractor();

        private ISchemaManagerProvider _schemaManagerProvider;
        private ILanguagesProvider _languagesProvider;

        public EntryPoint()
            : this(new SchemaManagerProvider(), new LanguagesProvider())
        { }

        public EntryPoint(ISchemaManagerProvider schemaManagerSingleton, ILanguagesProvider languagesProvider)
        {
            _schemaManagerProvider = schemaManagerSingleton;
            _languagesProvider = languagesProvider;
        }

        public override async Task<HopexResponse> Execute(InputArguments args)
        {
            try
            {
                var environmentId = Utils.GetEnvironmentId(HopexContext);
                var schema = ExtractSchemaPath(GetMegaRoot(), HopexContext.Request.Path, environmentId);

                if (schema == null)
                {
                    var ex = new Exception($"Unknown route: {HopexContext.Request.Path}");
                    Logger?.LogError(ex);
                    if (Utils.IsRunningInHAS)
                    {
                        return HopexResponse.Error(500, ex.Message);
                    }
                    return HopexResponse.Error((int)HttpStatusCode.BadRequest, JsonConvert.SerializeObject(new
                    {
                        HttpStatusCode = HttpStatusCode.BadRequest,
                        Error = ex.Message
                    }));
                }

                var schemaManager = await _schemaManagerProvider.GetInstanceAsync(Logger, HopexContext, schema.Version);

                var languages = _languagesProvider.GetLanguages(HopexContext.NativeRoot);

                var (graphQlSchema, hopexSchema) = await schemaManager.GetSchemaAsync(schema, languages);

                var executor = new DocumentExecuter();
                Inputs variables = null;
                if (args.Variables != null)
                {
                    variables = new Inputs(args.Variables);
                }

                Logger.LogInformation("Executing query");
                var root = CreateDataModel(hopexSchema);
                ExecutionResult result = null;
                try
                {
                    result = await executor.ExecuteAsync(_ =>
                    {
                        _.Schema = graphQlSchema;
                        _.Root = root;
                        _.UserContext = new UserContext
                        {
                            MegaRoot = GetNativeMegaRoot(),
                            IRoot = GetMegaRoot(),
                            WebServiceUrl = args.WebServiceUrl,
                            Schema= schema,
                            Languages = languages
                        };
                        _.OperationName = args.OperationName;
                        _.Query = args.Query;
                        _.Inputs = variables;
                    });

                    Logger.LogInformation($"Query terminated: {args.Query.Replace("\n", " ")}");

                    var writer = new DocumentWriter(true);
                    if (Utils.IsRunningInHAS)
                    {
                        return HopexResponse.Json(writer.Write(result));
                    }
                    else
                    {
                        return HopexResponse.Json(JsonConvert.SerializeObject(new
                        {
                            HttpStatusCode = HttpStatusCode.OK,
                            Result = writer.Write(result)
                        }));
                    }
                }
                finally
                {
                    if (result?.Errors != null)
                    {
                        foreach (var error in result.Errors)
                        {
                            Logger.LogInformation($"Query terminated with errors: {error.Message}");
                        }
                    }
                    if ((result?.Operation?.OperationType ?? OperationType.Query) == OperationType.Mutation)
                    {
                        PublishSession();
                    }
                    (root as IDisposable)?.Dispose();
                }
            }
            catch (ValidationException ex)
            {
                var errorMessage = $"{ex.Message}: {string.Join(" | ", ex.ValidationContext.Errors)}";
                Logger?.LogError(ex, errorMessage);
                if (Utils.IsRunningInHAS)
                {
                    return HopexResponse.Error((int)HttpStatusCode.InternalServerError, errorMessage);
                }
                return HopexResponse.Error((int)HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new
                {
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Error = errorMessage
                }));
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                if (Utils.IsRunningInHAS)
                {
                    return HopexResponse.Error((int)HttpStatusCode.InternalServerError, ex.Message);
                }
                return HopexResponse.Error((int)HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new
                {
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Error = ex.Message
                }));
            }
        }

        protected virtual void PublishSession()
        {
            var result = GetMegaRoot().CallFunctionString("~lcE6jbH9G5cK", "{\"instruction\":\"POSTPUBLISHINSESSION\"}");
            if (result == null)
            {
                throw new Exception("Publish has no result");
            }

            var infoLogged = (result == "") ? "Session publishing" : "Session already publishing";
            Logger.LogInformation(infoLogged);
        }

        protected virtual MegaRoot GetNativeMegaRoot()
        {
            return MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
        }

        public virtual IMegaRoot GetMegaRoot()
        {
            return RealMegaRootFactory.FromNativeRoot(HopexContext.NativeRoot);
        }

        protected virtual IHopexDataModel CreateDataModel(IHopexMetaModel metaModel)
        {
            var wrapper = GetNativeMegaRoot();
            return new HopexDataModel(metaModel, wrapper, GetMegaRoot(), Logger);
        }
        
        private SchemaReference ExtractSchemaPath(IMegaRoot iRoot, string requestPath, string environmentId)
        {
            return _schemaExtractor.Extract(iRoot, requestPath, WebServiceRoute, environmentId);
        }
    }
}