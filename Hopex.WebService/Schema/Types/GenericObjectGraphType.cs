using GraphQL;
using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    internal class GenericObjectInterface : InterfaceGraphType<IModelElement>
    {
        private readonly IGraphType _languagesType;
        private readonly Dictionary<MegaId, ObjectGraphType<IModelElement>> _concreteGraphTypes = new Dictionary<MegaId, ObjectGraphType<IModelElement>>();

        public GenericObjectGraphType WildcardType { get; set; }

        internal GenericObjectInterface(global::GraphQL.Types.Schema schema, IGraphType languagesType)
        {
            _languagesType = languagesType;
            WildcardType = new GenericObjectGraphType();

            schema.RegisterType(this);
            schema.RegisterType(WildcardType);

            Name = "GraphQLObjectInterface";

            AddCustomField(this);
            AddCustomRelationship(this);

            ResolveType = obj => ResolveConcreteType((IModelElement)obj);

            ImplementInType(WildcardType);            
        }

        private IObjectGraphType ResolveConcreteType(IModelElement modelElement)
        {
            var megaObject = modelElement.IMegaObject;
            var metaclassId = megaObject.GetClassId();
            var toolkit = megaObject.Root.CurrentEnvironment.Toolkit;
            var matchedConcreteType = _concreteGraphTypes.FirstOrDefault(pair => toolkit.IsSameId(pair.Key, metaclassId));
            if (matchedConcreteType.Key != null)
                return matchedConcreteType.Value;
            return WildcardType;
        }

        internal void ImplementInConcreteType(string metaclassId, ObjectGraphType<IModelElement> typeToEnrich, bool baseClass)
        {
            if(baseClass)
            {
                _concreteGraphTypes.Add(metaclassId, typeToEnrich);
            }
            ImplementInType(typeToEnrich);
        }

        private void ImplementInType(ObjectGraphType<IModelElement> typeToEnrich)
        {
            AddCustomField(typeToEnrich);
            AddCustomRelationship(typeToEnrich);
            typeToEnrich.AddResolvedInterface(this);
        }

        private void AddCustomField(ComplexGraphType<IModelElement> typeToEnrich)
        {
            typeToEnrich.Field<StringGraphType>(
                "CustomField",
                "Generic access to properties not defined in the schema",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" },
                    new QueryArgument(_languagesType) { Name = "language" },
                    new QueryArgument<FormatsEnumerationGraphType> { Name = "format" }
                ),
                ctx =>
                {
                    return ctx.Source.GetGenericValue(ctx.GetArgument<string>("id"), ctx.Arguments);
                }
            );
        }

        private static void AddCustomRelationship(ComplexGraphType<IModelElement> typeToEnrich)
        {
            typeToEnrich.Field<ListGraphType<GenericObjectInterface>>(
                "CustomRelationship",
                "Generic access to relationships not defined in the schema",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "relationId" },
                    new QueryArgument<IntGraphType> { Name = "first" },
                    new QueryArgument<StringGraphType> { Name = "after" },
                    new QueryArgument<IntGraphType> { Name = "last" },
                    new QueryArgument<StringGraphType> { Name = "before" },
                    new QueryArgument<IntGraphType> { Name = "skip" }
                ),
                ctx =>
                {
                    var collectionMegaId = ctx.GetArgument<string>("relationId");
                    var root = ((UserContext)ctx.UserContext["usercontext"]).IRoot;
                    var permissions = CrudComputer.GetCollectionMetaPermission(root, collectionMegaId);
                    if (!permissions.IsReadable)
                        return Enumerable.Empty<IModelElement>().GetEnumerator();
                    var collection = ctx.Source.GetGenericCollection(collectionMegaId);
                    var orderedCollection = collection.ToList();
                    return SchemaBuilder.GetPaginatedModelElements(orderedCollection, ctx.Arguments);
                }
            );
        }

    }

    internal class GenericObjectGraphType : ObjectWithParentGraphType<IModelElement>
    {
        internal GenericObjectGraphType()
        {
            Name = "GraphQLObject";
        }
    }
}
