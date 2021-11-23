using GraphQL;
using GraphQL.Language.AST;
using GraphQL.NewtonsoftJson;
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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

        private readonly ISchemaManagerProvider _schemaManagerProvider;
        private readonly ILanguagesProvider _languagesProvider;

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
                Logger.LogInformation("GraphQL macro start");

                //PropertyCache.ResetCache();

                var megaRoot = GetMegaRoot();
                var environmentId = Utils.GetEnvironmentId(HopexContext);
                var schema = ExtractSchemaPath(megaRoot, HopexContext.Request.Path, environmentId);
                if (schema == null)
                {
                    var ex = new Exception($"Unknown route: {HopexContext.Request.Path}");
                    Logger?.LogError(ex);
                    if (Utils.IsRunningInHAS)
                    {
                        return HopexResponse.Error(500, ex.Message);
                    }
                    return HopexResponse.Error((int) HttpStatusCode.BadRequest, JsonConvert.SerializeObject(new
                    {
                        HttpStatusCode = HttpStatusCode.BadRequest,
                        Error = ex.Message
                    }));
                }
                Logger.LogInformation("Get MegaRoot, EnvironmentId and SchemaPath terminated");

                var languages = _languagesProvider.GetLanguages(Logger, megaRoot);
                var currencies = _languagesProvider.GetCurrencies(Logger, megaRoot);
                Logger.LogInformation("Get Languages and currencies terminated");

                Logger.LogInformation("Schemas initialization start");
                var schemaManager = await _schemaManagerProvider.GetInstanceAsync(HopexContext, schema.Version, Logger);
                var (graphQlSchema, hopexSchema) = await schemaManager.GetSchemaAsync(megaRoot, schema, languages, currencies);
                Logger.LogInformation("Schemas initialization terminated");

                var root = CreateDataModel(hopexSchema);
                var executor = new DocumentExecuter();
                Inputs variables = null;
                if (args.Variables != null)
                {
                    variables = new Inputs(args.Variables);
                }
                Logger.LogInformation("CreateDataModel and DocumentExecuter terminated");

                ExecutionResult result = null;
                try
                {
                    Logger.LogInformation("Query start");
                    result = await executor.ExecuteAsync(_ =>
                    {
                        _.Schema = graphQlSchema;
                        _.Root = root;
                        _.UserContext = new Dictionary<string, object>
                        {
                            {
                                "usercontext", new UserContext
                                {
                                    MegaRoot = GetNativeMegaRoot(),
                                    IRoot = megaRoot,
                                    WebServiceUrl = args.WebServiceUrl,
                                    Schema = schema,
                                    Languages = languages
                                }
                            }
                        };
                        _.OperationName = args.OperationName;
                        _.Query = args.Query;
                        _.Inputs = variables;
                        //_.FieldMiddleware.Use<InstrumentFieldMiddleware>();
                    });
                    Logger.LogInformation($"Query terminated: {args.Query.Replace(Environment.NewLine, " ").Replace("\n", " ")}");

                    if (Utils.IsRunningInHAS)
                    {
                        return HopexResponse.Json(await WriteResultAsync(result));
                    }
                    else
                    {
                        return HopexResponse.Json(JsonConvert.SerializeObject(new
                        {
                            HttpStatusCode = HttpStatusCode.OK,
                            Result = await WriteResultAsync(result)
                        }));
                    }
                }
                finally
                {
                    if (result?.Errors != null)
                    {
                        foreach (var error in result.Errors)
                        {
                            Logger.LogInformation($"Query terminated with error(s): {error}");
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
                    return HopexResponse.Error((int) HttpStatusCode.InternalServerError, errorMessage);
                }
                return HopexResponse.Error((int) HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new
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
                    return HopexResponse.Error((int) HttpStatusCode.InternalServerError, ex.Message);
                }
                return HopexResponse.Error((int) HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new
                {
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Error = ex.Message
                }));
            }
            finally
            {
                //PropertyCache.ResetCache();
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

        private async Task<string> WriteResultAsync(ExecutionResult result)
        {
            var writer = new DocumentWriter(true);
            using(var stream = new MemoryStream())
            {
                await writer.WriteAsync(stream, result);
                stream.Position = 0;
                using(var reader = new StreamReader(stream, new UTF8Encoding(false)))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
