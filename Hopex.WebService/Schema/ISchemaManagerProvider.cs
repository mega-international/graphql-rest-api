using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Convertors;
using Hopex.Model.PivotSchema.Loaders;
using Mega.Macro.API;
using System;
using System.IO;
using System.Reflection;

namespace Hopex.Modules.GraphQL.Schema
{

    public interface ISchemaManagerProvider
    {
        GraphQLSchemaManager GetInstance(ILogger logger, IHopexContext hopexContext);
    }

    public class SchemaManagerProvider : ISchemaManagerProvider
    { 
        private static GraphQLSchemaManager Instance;

        public GraphQLSchemaManager GetInstance(ILogger logger, IHopexContext hopexContext)
        {
            if (Instance == null)
            {
                WriteLogFilenameToMegaErr(hopexContext);
                logger.LogInformation("Initializing schemas.");
                var hopexSchemaManager = new HopexMetaModelManager(CreateSchemaLoader(), ctx => new PivotConvertor(ctx));
                Instance = new GraphQLSchemaManager(hopexSchemaManager, logger);
                logger.LogInformation("Schemas initialized.");
            }
            return Instance;
        }

        private IPivotSchemaLoader CreateSchemaLoader()
        {
            return new FileSystemLoader($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CONFIG");
        }

        protected virtual void WriteLogFilenameToMegaErr(IHopexContext hopexContext)
        {
            var megaRoot = MegaWrapperObject.Cast<MegaRoot>(hopexContext.NativeRoot);
            megaRoot.CurrentEnvironment.Site.NativeObject.Debug.LogWrite(Environment.NewLine + $@"#Hopex-[Macro]:C:\ProgramData\MEGA\Logs\Hopex-[Macro]-{DateTime.Now.Date:yyyyMMdd}.log" + Environment.NewLine);
        }
    }
}
