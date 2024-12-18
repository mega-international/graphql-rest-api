using GraphQL.Execution;
using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static Hopex.Model.MetaModel.Constants;

namespace Hopex.Modules.GraphQL.Schema
{
    internal class SchemaMappingResolver
    {
        private GraphQLSchemaManager _schemaManager;

        internal SchemaMappingResolver(GraphQLSchemaManager schemaManager)
        {
            _schemaManager = schemaManager;
        }

        const string MCID_METACLASS = "~P20000000c10";

        internal void CreateField(IClassDescription entity, ObjectGraphType<IModelElement> type)
        {
            if (entity.Id == MCID_METACLASS)
                type.Field<ListGraphType<SchemaGraphType>>(
                        "schemas",
                        "Included in schemas",
                        resolve: ctx => ResolveMetaClass(ctx.Source)
                    );
        }

        private IEnumerable<SchemaType> ResolveMetaClass(IModelElement elem)
        {
            return _schemaManager.HopexSchemas
                .Select(schema => new { schema, classDescription = schema.FindClassDescriptionById(elem.Id) })
                .Where(pair => pair.classDescription != null && pair.classDescription.IsEntryPoint)                
                .Select(pair => new SchemaType(pair.schema, pair.classDescription.Name)).ToList();
        }

        internal void CreateField(IRelationshipDescription link, ObjectGraphType<IModelElement> tc)
        {
            if(link.RoleId == MAEID_METACLASS_METAATRBIBUTE)
            {
                tc.Field<ListGraphType<SchemaGraphType>>(
                    "schemas",
                    "Included in schemas",
                    resolve: ctx => ResolveMetaAttribute(ctx.Source)
                );
            }
        }

        private IEnumerable<SchemaType> ResolveMetaAttribute(IModelElement elem)
        {
            //Si on veut créer ce champ, il faudrait plutôt récupérer la liste des classes associées via les collections et non avec un Parent qui est faux
            var metaclassId = elem.Parent.Id;
            return _schemaManager.HopexSchemas
                .Select(schema => new { schema, propertyDescription = schema.FindClassDescriptionById(metaclassId)?.FindPropertyDescriptionById(elem.Id) })
                .Where(pair => pair.propertyDescription != null)
                .Select(pair => new SchemaType(pair.schema, pair.propertyDescription.Name));
        }

        internal void AddSchemaQueryArgument(QueryArguments arguments, string roleId)
        {
            arguments.Add(new QueryArgument<ListGraphType<StringGraphType>>
            {
                Name = "fromSchema",
            });
        }

        internal Func<IModelElement, bool> CreateSchemaPredicate(IDictionary<string, ArgumentValue> arguments)
        {
            if (arguments.TryGetValue("fromSchema", out var obj) && obj.Value != null)
            {
                var candidateSchemas = ((IEnumerable<object>)obj.Value).Cast<string>()
                        .SelectMany(name => _schemaManager.HopexSchemas.Where(schema => schema.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
                return metaclass => candidateSchemas.Any(schema => schema.FindClassDescriptionById(metaclass.Id) != null);
            }
            return null;
        }
    }
}
