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

        public IEnumerable<IHopexMetaModel> Schemas => _schemas.Values;

        public async Task<IHopexMetaModel> GetMetaModelAsync(SchemaReference schemaRef, ValidationContext ctx = null)
        {
            if (_schemas.TryGetValue(schemaRef.UniqueId, out IHopexMetaModel schema))
            {
                return schema;
            }

            if (_resolvedSchemas.Contains(schemaRef.UniqueId))
            {
                throw new Exception("Circular reference");
            }

            if(_loader == null)
            {
                throw new Exception("Unable to load schema.");
            }
            var json = await _loader.ReadAsync(schemaRef);
            if (json == null)
            {
                throw new Exception($"Schema {schemaRef.UniqueId} not found.");
            }

            var convertor = _convertorFactory(ctx ?? new ValidationContext());
            if (convertor != null)
            {
                schema = await convertor.ConvertAsync(this, json, schemaRef);
                _schemas[schemaRef.UniqueId] = schema;
                _resolvedSchemas.Add(schemaRef.UniqueId);
            }

            return schema;
        }

        public async Task LoadAllAsync(string version)
        {
            foreach (var schemaRef in _loader.EnumerateStandardSchemas(version))
            {
                try
                {
                    await GetMetaModelAsync(schemaRef);
                }
                catch
                {
                    // TODO ?
                }
            }
        }
    }
}
