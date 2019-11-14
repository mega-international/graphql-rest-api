using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.PivotSchema.Convertors;

namespace Hopex.Model.MetaModel
{
    public class HopexMetaModelManager : IHopexMetaModelManager
    {
        private readonly Dictionary<string, IHopexMetaModel> _schemas = new Dictionary<string, IHopexMetaModel>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _resolvedSchemas = new HashSet<string>();
        private readonly IPivotSchemaLoader _loader;
        private readonly Func<ValidationContext, IPivotSchemaConvertor> _convertorFactory;

        public HopexMetaModelManager(IPivotSchemaLoader loader, Func<ValidationContext, IPivotSchemaConvertor> convertorFactory)
        {
            _loader = loader;
            _convertorFactory = convertorFactory;
        }

        public async Task<IHopexMetaModel> GetMetaModelAsync(string schemaName, ValidationContext ctx = null)
        {
            if (_schemas.TryGetValue(schemaName, out IHopexMetaModel schema))
            {
                return schema;
            }

            if (_resolvedSchemas.Contains(schemaName))
            {
                throw new Exception("Circular reference");
            }
            PivotSchema.Models.PivotSchema json = await _loader.ReadAsync(schemaName);
            if (json == null)
            {
                throw new Exception($"Schema {schemaName} not found.");
            }

            var convertor = _convertorFactory(ctx ?? new ValidationContext());
            schema = await convertor.ConvertAsync(this, json);
            _schemas[schemaName] = schema;
            _resolvedSchemas.Add(schemaName);
            return schema;
        }
    }
}
