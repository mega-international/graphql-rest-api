using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Convertors;
using Hopex.Model.PivotSchema.Loaders;
using Hopex.Modules.GraphQL.Schema;
using Mega.Macro.API;

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
        private static GraphQLSchemaManager _schemaManager;

        public override async Task<HopexResponse> Execute(InputArguments args)
        {
            try
            {
                if (_schemaManager == null)
                {
                    Logger.LogInformation("Initializing schemas.");
                    var hopexSchemaManager = new HopexMetaModelManager(CreateSchemaLoader(), ctx => new PivotConvertor(ctx));
                    _schemaManager = new GraphQLSchemaManager(hopexSchemaManager);
                    Logger.LogInformation("Schemas initialized.");
                }

                var schemaName = ExtractSchemaName();
                var (graphQlSchema, hopexSchema) = await _schemaManager.GetSchemaAsync(schemaName);

                var executer = new DocumentExecuter();
                var query = args.query;
                Inputs variables = null;
                if (args.variables != null)
                {
                    variables = new Inputs(args.variables);
                }

                Logger.LogInformation("Executing query");
                var root = CreateDataModel(hopexSchema);
                try
                {
                    var result = await executer.ExecuteAsync(_ =>
                    {
                        _.UserContext = HopexContext.Request;
                        _.Schema = graphQlSchema;
                        _.Root = root;
                        _.Query = query;
                        _.Inputs = variables;
                    });

                    Logger.LogInformation("Query terminated.");

                    var writer = new DocumentWriter(true);
                    return HopexResponse.Text(writer.Write(result), "application/json");
                }
                finally
                {
                    var result =((dynamic) HopexContext.NativeRoot).CallFunction(
                        "~lcE6jbH9G5cK[PublishStayInSessionWizard Command Launcher]",
                        "{\"instruction\":\"PUBLISHINSESSION\"}");
                    if (!result.ToString().Contains("SESSION_PUBLISH"))
                    {
                        Logger.LogError(new Exception("Session wasn't published"));
                    }
                    Logger.LogInformation("Session published");
                    (root as IDisposable)?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                return HopexResponse.Text(ex.Message, statusCode: 500);
            }
        }

        protected virtual IPivotSchemaLoader CreateSchemaLoader()
        {
            return new FileSystemLoader($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CONFIG");
        }

        protected virtual IHopexDataModel CreateDataModel(IHopexMetaModel metaModel)
        {
            var wrapper = MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
            return new HopexDataModel(metaModel, wrapper);
        }

        private string ExtractSchemaName()
        {
            return HopexContext.Request.Path.Substring($"/api/{WebServiceRoute}/".Length);
        }
    }
}
