using GraphQL;
using GraphQL.Http;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.Mocks;
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

        private ISchemaManagerProvider _schemaManagerProvider;

        public EntryPoint()
            :this(new SchemaManagerProvider())
        {}

        public EntryPoint(ISchemaManagerProvider schemaManagerSingleton)
        {
            _schemaManagerProvider = schemaManagerSingleton;
        }

        public override async Task<HopexResponse> Execute(InputArguments args)
        {
            try
            {
                var schemaManager = _schemaManagerProvider.GetInstance(Logger, HopexContext);
                var environmentId = GetEnvironmentId();
                var schemaPath = ExtractSchemaPath(GetMegaRoot(), HopexContext.Request.Path, environmentId);
                if (string.IsNullOrEmpty(schemaPath))
                {
                    var ex = new Exception($"Unknown route: {HopexContext.Request.Path}");
                    Logger?.LogError(ex);
                    return HopexResponse.Error((int)HttpStatusCode.BadRequest, JsonConvert.SerializeObject(new { HttpStatusCode = HttpStatusCode.BadRequest, Error = ex.Message }));
                }
                var (graphQlSchema, hopexSchema) = await schemaManager.GetSchemaAsync(schemaPath);

                var executer = new DocumentExecuter();
                Inputs variables = null;
                if(args.Variables != null)
                {
                    variables = new Inputs(args.Variables);
                }

                Logger.LogInformation("Executing query");
                var root = CreateDataModel(hopexSchema);
                try
                {
                    var result = await executer.ExecuteAsync(_ =>
                    {
                        _.Schema = graphQlSchema;
                        _.Root = root;
                        _.UserContext = new UserContext
                        {
                            MegaRoot = GetNativeMegaRoot(),
                            IRoot = GetMegaRoot(),
                            WebServiceUrl = args.WebServiceUrl
                        };
                        _.OperationName = args.OperationName;
                        _.Query = args.Query;
                        _.Inputs = variables;
                        _.ThrowOnUnhandledException = true;
                    });

                    Logger.LogInformation($"Query terminated: {args.Query.Replace("\n", " ")}");

                    var writer = new DocumentWriter(true);
                    return HopexResponse.Json(JsonConvert.SerializeObject(new { HttpStatusCode = HttpStatusCode.OK, Result = writer.Write(result) }));
                }
                finally
                {
                    //PublishSession();
                    (root as IDisposable)?.Dispose();
                }
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex);
                return HopexResponse.Error((int)HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new { HttpStatusCode = HttpStatusCode.InternalServerError, Error = ex.Message }));
            }
        }

        protected virtual void PublishSession()
        {
            var result = GetMegaRoot().CallFunctionString("~lcE6jbH9G5cK", "{\"instruction\":\"PUBLISHINSESSION\"}");
            if (result == null || !result.Contains("SESSION_PUBLISH"))
            {
                throw new Exception("Session wasn't published");
            }

            Logger.LogInformation("Session published");
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
            var wrapper = MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
            return new HopexDataModel(metaModel, wrapper, Logger);
        }

        protected virtual string GetEnvironmentId()
        {
            if (HopexContext.Request.Headers.ContainsKey("EnvironmentId"))
            {
                if (HopexContext.Request.Headers["EnvironmentId"].Length > 0)
                {
                    return HopexContext.Request.Headers["EnvironmentId"][0];
                }
            }
            return "";
        }

        private string ExtractSchemaPath(IMegaRoot iRoot, string requestPath, string environmentId)
        {
            return new SchemaPathExtractor().Extract(iRoot, requestPath, environmentId, WebServiceRoute, Logger);            
        }
    }
}
