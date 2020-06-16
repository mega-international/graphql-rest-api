using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;
using System.Collections.Generic;
using System.Linq;
using Mega.Macro.API.Library;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    internal class GenericObjectInterface : InterfaceGraphType<IModelElement>
    {
        private readonly IGraphType _languagesType;
        private readonly GenericObjectGraphType _wildcardType;
        private readonly Dictionary<MegaId, ObjectGraphType<IModelElement>> _concreteGraphTypes = new Dictionary<MegaId, ObjectGraphType<IModelElement>>();

        private static readonly Dictionary<string, object> NO_ARGUMENTS = new Dictionary<string, object>();

        internal GenericObjectInterface(global::GraphQL.Types.Schema schema, IGraphType languagesType)
        {
            _languagesType = languagesType;
            _wildcardType = new GenericObjectGraphType();

            schema.RegisterType(this);
            schema.RegisterType(_wildcardType);

            Name = "GraphQLObjectInterface";
            Field<StringGraphType>("id", "Absolute identifier", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.AbsoluteIdentifier, ctx.Arguments ?? NO_ARGUMENTS));
            Field<StringGraphType>("name", "Name", resolve: ctx =>  ctx.Source.GetGenericValue(MetaAttributeLibrary.ShortName, ctx.Arguments ?? NO_ARGUMENTS));
            Field<DateGraphType>("creationDate", "Creation date", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.CreationDate, ctx.Arguments ?? NO_ARGUMENTS));
            Field<DateGraphType>("modificationDate", "Modification date", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.ModificationDate, ctx.Arguments ?? NO_ARGUMENTS));

            AddCustomField(this);
            AddCustomRelationship(this);

            ResolveType = obj => ResolveConcreteType((IModelElement)obj);

            ImplementInType(_wildcardType);            
        }

        private IObjectGraphType ResolveConcreteType(IModelElement modelElement)
        {
            var megaObject = modelElement.IMegaObject;
            var metaclassId = megaObject.GetClassId();
            var toolkit = megaObject.Root.CurrentEnvironment.Toolkit;
            var matchedConcreteType = _concreteGraphTypes.FirstOrDefault(pair => toolkit.IsSameId(pair.Key, metaclassId));
            if (matchedConcreteType.Key != null)
                return matchedConcreteType.Value;
            return _wildcardType;
        }

        internal void ImplementInConcreteType(string metaclassId, ObjectGraphType<IModelElement> typeToEnrich)
        {
            _concreteGraphTypes.Add(metaclassId, typeToEnrich);
            ImplementInType(typeToEnrich);
        }

        private void ImplementInType(ObjectGraphType<IModelElement> typeToEnrich)
        {
            typeToEnrich.AddResolvedInterface(this);
            AddCustomField(typeToEnrich);
            AddCustomRelationship(typeToEnrich);
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
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" },
                    new QueryArgument<IntGraphType> { Name = "first" },
                    new QueryArgument<StringGraphType> { Name = "after" },
                    new QueryArgument<IntGraphType> { Name = "last" },
                    new QueryArgument<StringGraphType> { Name = "before" },
                    new QueryArgument<IntGraphType> { Name = "skip" }
                ),
                ctx =>
                {
                    var collectionMegaId = ctx.GetArgument<string>("id");
                    var root = ((UserContext)ctx.UserContext).IRoot;
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

    internal class GenericObjectGraphType : ObjectGraphType<IModelElement>
    {
        private static readonly Dictionary<string, object> NO_ARGUMENTS = new Dictionary<string, object>();

        internal GenericObjectGraphType()
        {
            Name = "GraphQLObject";
            Field<StringGraphType>("id", "Absolute identifier", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.AbsoluteIdentifier, ctx.Arguments ?? NO_ARGUMENTS));
            Field<StringGraphType>("name", "Name", resolve: ctx =>  ctx.Source.GetGenericValue(MetaAttributeLibrary.ShortName, ctx.Arguments ?? NO_ARGUMENTS));
            Field<DateGraphType>("creationDate", "Creation date", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.CreationDate, ctx.Arguments ?? NO_ARGUMENTS));
            Field<DateGraphType>("modificationDate", "Modification date", resolve: ctx => ctx.Source.GetGenericValue(MetaAttributeLibrary.ModificationDate, ctx.Arguments ?? NO_ARGUMENTS));
        }
    }
}
