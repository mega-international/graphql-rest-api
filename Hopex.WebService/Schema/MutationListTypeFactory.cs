using GraphQL.Resolvers;
using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.DataModel;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema
{
    public class CollectionActionGraphType : EnumerationGraphType<CollectionAction>
    {
        public CollectionActionGraphType()
        {
            Name = "_InputCollectionActionEnum";
        }
    }

    internal class MutationListTypeFactory
    {
        private static readonly string [] _reimplementedFieldNames = new string [] { "order", "linkComment", "id", "idType", "creationMode" };
        private readonly SchemaBuilder _schemaBuilder;
        private readonly IGraphType _defaultType;
        private static int _seq = 1;

        public MutationListTypeFactory(SchemaBuilder schemaBuilder)
        {
            _schemaBuilder = schemaBuilder;
            _defaultType = GenerateMutationType();
        }

        private IGraphType GenerateMutationType(IEnumerable<GraphQLPropertyDescription> properties = null, int seq=0)
        { 
            var type = new InputObjectGraphType<Dictionary<string, object>>
            {
                Name = $"_InputCollectionAction{seq}"
            };
            type.AddField( new FieldType{
                ResolvedType=new CollectionActionGraphType(),
                Name="action",                
                Resolver= new FuncFieldResolver<Dictionary<string,object>, CollectionAction>(ctx => (CollectionAction)ctx.Source["action"])
            });

            var listType = new InputObjectGraphType<Dictionary<string, object>> { Name = $"_defaultActionElement{seq}" };
            listType.AddField(new FieldType
            {
                Type = typeof(IntGraphType),
                Name = "order",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, int>(ctx => (int)ctx.Source["order"])
            });
            listType.AddField(new FieldType
            {
                Type = typeof(StringGraphType),
                Name = "linkComment",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, string>(ctx => (string)ctx.Source["linkComment"])
            });
            listType.AddField(new FieldType
            {
                Type = typeof(StringGraphType),
                Name = "id",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, string>(ctx => (string)ctx.Source["id"])
            });
            listType.AddField(new FieldType
            {
                Type = typeof(IdType),
                Name = "idType",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, string>(ctx => (string)ctx.Source["idType"])
            });
            var enumCreationMode = new HopexEnumerationGraphType { Name = "creationMode" };
            enumCreationMode.AddValue("RAW", "Do not use InstanceCreator, but classic \"create mode\"", false);
            enumCreationMode.AddValue("BUSINESS", "Use InstanceCreator", true);
            listType.AddField(new FieldType
            {
                ResolvedType = enumCreationMode,
                Name = "creationMode",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, bool>(ctx => (bool)ctx.Source["creationMode"])
            });
            if(properties != null)
            {
                CreateProperties(properties, listType);
            }
            type.AddField(new FieldType
            {
                ResolvedType = new ListGraphType<InputObjectGraphType<Dictionary<string, object>>> { ResolvedType = listType },
                Name = "list",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, List<Dictionary<string, object>>>(ctx => (List<Dictionary<string, object>>)ctx.Source["list"])
            });
            return type;
        }

        internal IGraphType CreateMutationList(GraphQLRelationshipDescription relationship)
        {
            var targetClass = relationship.TargetClass;
            if (targetClass == null)
            {
                return _defaultType;
            }

            var properties = relationship.TargetClass.Properties
                .Where(p => !_reimplementedFieldNames.Any(n => p.Name.Equals(n, StringComparison.OrdinalIgnoreCase))).ToList();

            if (properties.Any())
            {
                return GenerateMutationType(properties, _seq++);
            }
            return _defaultType;
        }

        private void CreateProperties(IEnumerable<GraphQLPropertyDescription> properties, IComplexGraphType type)
        {
            _schemaBuilder.CreateProperties<Dictionary<string, object>>(properties, type, (ctx, prop) =>
            {
                string format = null;
                if(ctx.Arguments != null && ctx.Arguments.ContainsKey("format") && ctx.Arguments ["format"].Value != null)
                {
                    format = ctx.Arguments ["format"].Value.ToString();
                }
                var result = ctx.Source [prop.Name];
                if(result is DateTime date && !string.IsNullOrEmpty(format))
                {
                    return date.ToString(format);
                }
                return result;
            }, true);
        }
    }
}
