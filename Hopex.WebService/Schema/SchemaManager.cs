using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Modules.GraphQL.Schema
{

    public class GraphQLSchemaManager
    {
        private readonly IHopexMetaModelManager _hopexSchemaManager;

        public GraphQLSchemaManager(IHopexMetaModelManager hopexSchemaManager)
        {
            _hopexSchemaManager = hopexSchemaManager;
        }

        public async Task<(global::GraphQL.Types.Schema, IHopexMetaModel)> GetSchemaAsync(string name)
        {
            if (_schemas.TryGetValue(name, out var builder))
            {
                return (builder.Schema, builder.HopexSchema);
            }

            var hopexSchema = await _hopexSchemaManager.GetMetaModelAsync(name);
            if (hopexSchema == null)
            {
                return (null, null);
            }

            builder = new SchemaBuilder(hopexSchema);
            builder.Create();
            _schemas.Add(name, builder);
            return (builder.Schema, hopexSchema);
        }

        private readonly Dictionary<string, SchemaBuilder> _schemas = new Dictionary<string, SchemaBuilder>(StringComparer.OrdinalIgnoreCase);
    }
}
