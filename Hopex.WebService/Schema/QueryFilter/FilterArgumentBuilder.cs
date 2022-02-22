using GraphQL.Types;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    internal class FilterArgumentBuilder
    {
        private readonly SchemaBuilder _schemaBuilder;

        public FilterArgumentBuilder(SchemaBuilder schemaBuilder)
        {
            _schemaBuilder = schemaBuilder;
        }

        internal class FilterItem
        {
            public InputObjectGraphType<object> Filter;
            public HopexEnumerationGraphType OrderBy;
            public bool IsInitialized;
        }

        internal Dictionary<string, FilterItem> Filters { get; } = new Dictionary<string, FilterItem>();

        public QueryArguments BuildFilterArguments(GraphQLClassDescription graphQlClass)
        {
            var arguments = new QueryArguments();
            return AddFilterArguments(arguments, graphQlClass);
        }

        public QueryArguments AddFilterArguments(QueryArguments arguments, GraphQLClassDescription graphQlClass)
        {
            var clazz = graphQlClass.MetaClass;
            var filterItem = GetOrCreateFilterObject(clazz.Name);
            arguments.Add(new QueryArgument(filterItem.Filter) { Name = "filter" });
            arguments.Add(new QueryArgument(new ListGraphType(filterItem.OrderBy)) { Name = "orderBy" });

            if (filterItem.IsInitialized)
            {
                return arguments;
            }

            filterItem.IsInitialized = true;
            var filter = filterItem.Filter;
            var enumOrderBy = filterItem.OrderBy;
            filter.AddField(new FieldType
            {
                Name = "and",
                Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                ResolvedType = new ListGraphType(new NonNullGraphType(filter))
            });
            filter.AddField(new FieldType
            {
                Name = "or",
                Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                ResolvedType = new ListGraphType(new NonNullGraphType(filter))
            });

            foreach (var graphQlProperty in graphQlClass.Properties)
            {
                var prop = graphQlProperty.MetaAttribute;
                var propName = graphQlProperty.Name;
                var graphType = TypeExtensions.GetGraphTypeFromType(prop.NativeType);
                var typeList = typeof(ListGraphType<>).MakeGenericType(typeof(NonNullGraphType<>).MakeGenericType(graphType));

                IGraphType resolvedType = null;
                IGraphType resolvedListType = null;

                if (prop.EnumValues?.Count() > 0)
                {
                    resolvedType = _schemaBuilder.GetOrCreateEnumType(prop);
                    if (resolvedType != null)
                    {
                        resolvedListType = new ListGraphType(new NonNullGraphType(resolvedType));
                    }
                }

                AddFieldsToFilter(filter, prop.Id, propName, prop.PropertyType, graphType, typeList, resolvedType, resolvedListType);

                enumOrderBy.AddValue($"{propName}_ASC", $"Order by {prop.Name} ascending", new Tuple<string, int>(prop.Id, 1));
                enumOrderBy.AddValue($"{propName}_DESC", $"Order by {prop.Name} descending", new Tuple<string, int>(prop.Id, -1));
            }

            foreach (var graphQlRelationship in graphQlClass.Relationships)
            {
                var rel = graphQlRelationship.MetaAssociation;
                var targetId = rel.Path.Last().TargetSchemaId;
                var targetName = _schemaBuilder.HopexSchema.Classes.Where(x => x.Id == targetId).Select(x =>x.Name).FirstOrDefault();
                var defaultTargetFilter = new InputObjectGraphType<object> { Name = $"{targetName}Filter" };
                defaultTargetFilter.AddField(new FieldType { Name = $"defaultField_{Guid.NewGuid().ToString().Replace("-", "")}", Type = typeof(StringGraphType) });
                var targetFilter = GetOrCreateFilterObject(targetName, defaultTargetFilter );
                filter.AddField(new FieldType
                {
                    Name = rel.Name + "_some",
                    Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                    ResolvedType = new ListGraphType(new NonNullGraphType(targetFilter.Filter))
                });
                var countFilter = new InputObjectGraphType<object> {Name = "countFilter"};
                countFilter.AddField(new FieldType {Name = "count", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                countFilter.AddField(new FieldType {Name = "count_not", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                countFilter.AddField(new FieldType {Name = "count_lt", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                countFilter.AddField(new FieldType {Name = "count_lte", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                countFilter.AddField(new FieldType {Name = "count_gt", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                countFilter.AddField(new FieldType {Name = "count_gte", Type = typeof(IntGraphType), ResolvedType = new IntGraphType()});
                filter.AddField(new FieldType
                {
                    Name = rel.Name + "_count",
                    Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                    ResolvedType = countFilter
                });
                //filter.AddField(new FieldType
                //{
                //    Name = rel.Name + "_every",
                //    Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                //    ResolvedType = new ListGraphType(new NonNullGraphType(filter))
                //});
                //filter.AddField(new FieldType
                //{
                //    Name = rel.Name + "_none",
                //    Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                //    ResolvedType = new ListGraphType(new NonNullGraphType(filter))
                //});
            }

            return arguments;
        }

        private FilterItem GetOrCreateFilterObject(string className, InputObjectGraphType<object> defaultFilter = null)
        {
            var filterName = className + "Filter";
            var orderByName = className + "OrderBy";
            if (!Filters.TryGetValue(filterName, out var filter))
            {
                filter = new FilterItem
                {
                    Filter = defaultFilter ?? new InputObjectGraphType<object> { Name = filterName },
                    OrderBy = new HopexEnumerationGraphType {Name = orderByName },
                    IsInitialized = false
                };
                Filters.Add(filterName, filter);
            }
            return filter;
        }

        private static void AddFieldsToFilter(IComplexGraphType filter, string propertyId, string propertyName, PropertyType propertyType, Type graphType, Type typeList, IGraphType resolvedType, IGraphType resolvedListType)
        {
            switch (propertyType)
            {
                case PropertyType.Id:
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_not_in"});
                    break;
                case PropertyType.Enum:
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_not_in"});
                    break;
                case PropertyType.Int:
                case PropertyType.Long:
                case PropertyType.Double:
                case PropertyType.Date:
                case PropertyType.Currency:
                case PropertyType.Binary:
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_not_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lt"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lte"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gt"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gte"});
                    break;
                case PropertyType.String:
                case PropertyType.RichText:
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedListType, Type = typeList, Name = $"{propertyName}_not_in"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lt"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lte"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gt"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gte"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_contains"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_contains"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_starts_with"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_starts_with"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_ends_with"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_ends_with"});
                    break;
                case PropertyType.Boolean:
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}"});
                    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not"});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //Fields in common with all type
            filter.AddField(new FieldType { ResolvedType = null, Type = typeof(BooleanGraphType), Name = $"{propertyName}_empty" });
        }
    }
}
