using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Modules.GraphQL.Schema
{
    public class GraphQLSchemaManager
    {
        private readonly IHopexMetaModelManager _hopexSchemaManager;

        private ILogger Logger { get; }

        public GraphQLSchemaManager(IHopexMetaModelManager hopexSchemaManager, ILogger logger)
        {
            _hopexSchemaManager = hopexSchemaManager;
            Logger = logger;
        }

        private static readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

        public async Task<(global::GraphQL.Types.Schema, IHopexMetaModel)> GetSchemaAsync(SchemaReference schemaRef,  Dictionary<string, string> languages)
        {
            var path = schemaRef.UniqueId;
            if (_schemas.TryGetValue(path, out var builder))
            {
                return (builder.Schema, builder.HopexSchema);
            }

            await _loadSemaphore.WaitAsync();
            try
            {
                if (_schemas.TryGetValue(path, out builder))
                {
                    return (builder.Schema, builder.HopexSchema);
                }

                var hopexSchema = await _hopexSchemaManager.GetMetaModelAsync(schemaRef);
                if (hopexSchema == null)
                {
                    return (null, null);
                }

                builder = new SchemaBuilder(hopexSchema, languages, Logger, this);
                builder.Create();
                _schemas.Add(path, builder);
                return (builder.Schema, hopexSchema);
            }
            finally
            {
                _loadSemaphore.Release();
            }           
        }
        internal IEnumerable<IHopexMetaModel> HopexSchemas => _hopexSchemaManager.Schemas;

        private readonly Dictionary<string, SchemaBuilder> _schemas = new Dictionary<string, SchemaBuilder>(StringComparer.OrdinalIgnoreCase);
    }
}
