using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Convertors;
using Hopex.Model.PivotSchema.Loaders;

using Mega.Macro.API;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL.Schema
{
    public class SchemaManagerProvider : ISchemaManagerProvider
    {
        private static GraphQLSchemaManager Instance;

        public virtual async Task<GraphQLSchemaManager> GetInstanceAsync(IHopexContext hopexContext, string version, ILogger logger)
        {
            if (Instance == null)
            {
                WriteLogFilenameToMegaErr(hopexContext);
                var hopexSchemaManager = new HopexMetaModelManager(CreateSchemaLoader(), ctx => new PivotConvertor(ctx));
                await hopexSchemaManager.LoadAllAsync(version);
                Instance = new GraphQLSchemaManager(hopexSchemaManager, logger);
            }
            return Instance;
        }

        protected IPivotSchemaLoader CreateSchemaLoader()
        {
            var folder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CONFIG";
            if (!Directory.Exists(folder))
            {
                throw new Exception($"Standard schemas folder {folder} does not exist.");
            }
            var standardSchemasLoader = new FileSystemLoader(folder);
            if (!Utils.IsRunningInHAS)
            {
                return standardSchemasLoader;
            }

            return new FileSystemLoader(Utils.HASCustomSchemasFolder, standardSchemasLoader);
        }

        protected virtual void WriteLogFilenameToMegaErr(IHopexContext hopexContext)
        {
            var megaRoot = MegaWrapperObject.Cast<MegaRoot>(hopexContext.NativeRoot);
            megaRoot.CurrentEnvironment.Site.NativeObject.Debug.LogWrite(Environment.NewLine + $@"#Hopex-[WebService-GraphQL]:C:\ProgramData\MEGA\Logs\Hopex-[WebService-GraphQL]-{DateTime.Now.Date:yyyyMMdd}.log" + Environment.NewLine);
            megaRoot.CurrentEnvironment.Site.NativeObject.Debug.LogWrite(Environment.NewLine + $@"#Hopex-[Macro]:C:\ProgramData\MEGA\Logs\Hopex-[Macro]-{DateTime.Now.Date:yyyyMMdd}.log" + Environment.NewLine);

        }
    }
}
