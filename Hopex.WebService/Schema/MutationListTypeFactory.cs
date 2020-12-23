using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
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
        private readonly SchemaBuilder _schemaBuilder;
        private readonly global::GraphQL.Types.Schema _graphQLSchema;
        private readonly IHopexMetaModel _hopexSchema;
        private readonly IGraphType _defaultType;
        private static int _seq = 1;

        public MutationListTypeFactory(SchemaBuilder schemaBuilder, global::GraphQL.Types.Schema graphQLSchema, IHopexMetaModel hopexSchema)
        {
            _schemaBuilder = schemaBuilder;
            _graphQLSchema = graphQLSchema;
            _hopexSchema = hopexSchema;
            _defaultType = GenerateMutationType();
        }

        private IGraphType GenerateMutationType(IEnumerable<IPropertyDescription> linkProperties=null, int seq=0)
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
                Type = typeof(int).GetGraphTypeFromType(true),
                Name = "order",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, int>(ctx => (int)ctx.Source["order"])
            });
            listType.AddField(new FieldType
            {
                Type = typeof(string).GetGraphTypeFromType(true),
                Name = "linkComment",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, string>(ctx => (string)ctx.Source["linkComment"])
            });
            listType.AddField(new FieldType
            {
                Type = typeof(string).GetGraphTypeFromType(true),
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
            if (linkProperties!=null)
            {
                _schemaBuilder.CreateProperties<Dictionary<string,object>>(linkProperties, listType, (ctx, prop) =>
                {
                    string format = null;
                    if(ctx.Arguments != null && ctx.Arguments.ContainsKey("format"))
                    {
                         format = ctx.Arguments["format"].ToString();
                    }
                    var result = ctx.Source[prop.Name];
                    if (result is DateTime date && !string.IsNullOrEmpty(format))
                    {
                        return date.ToString(format);
                    }
                    return result;
                }, true);
            }
            type.AddField(new FieldType
            {
                ResolvedType = new ListGraphType<NonNullGraphType> { ResolvedType = listType },                
                Name = "list",
                Resolver = new FuncFieldResolver<Dictionary<string, object>, List<Dictionary<string, object>>>(ctx => (List<Dictionary<string, object>>)ctx.Source["list"])
            });
            return type;
        }

        internal IGraphType CreateMutationList(IClassDescription targetClass)
        {
            if (targetClass == null)
            {
                return _defaultType;
            }
            var linkProperties = targetClass.Properties.Where(p => (p.Scope == PropertyScope.TargetClass || p.Scope == PropertyScope.Class) && !p.IsReadOnly);
            if (linkProperties.Any())
            {
                return GenerateMutationType(linkProperties, _seq++);
            }
            return _defaultType;
        }
    }
}
