using GraphQL;
using GraphQL.Types;

using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Hopex.Modules.GraphQL.Schema.Types;
using Mega.Macro.API.Library;

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

        public QueryArguments BuildFilterArguments(IClassDescription clazz, IEnumerable<IPropertyDescription> linkProperties = null)
        {
            var arguments = new QueryArguments();
            return AddFilterArguments(arguments, clazz, linkProperties);
        }

        public QueryArguments AddFilterArguments(QueryArguments arguments, IClassDescription clazz, IEnumerable<IPropertyDescription> linkProperties = null)
        {
            var filterItem = GetOrCreateFilterObject(clazz.Name);
            arguments.Add(new QueryArgument(filterItem.Filter) { Name = "filter" });
            arguments.Add(new QueryArgument<IntGraphType> { Name = "first" });
            arguments.Add(new QueryArgument<StringGraphType> { Name = "after" });
            arguments.Add(new QueryArgument<IntGraphType> { Name = "last" });
            arguments.Add(new QueryArgument<StringGraphType> { Name = "before" });
            arguments.Add(new QueryArgument<IntGraphType> { Name = "skip" });
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

            var properties = clazz.Properties;
            if (linkProperties != null)
            {
                properties = properties.Concat(linkProperties);
            }
            foreach (var prop in properties)
            {
                var propName = prop.Name.ToCamelCase();
                if (false)//(prop.PropertyType == PropertyType.Id && prop.Id != MetaAttributeLibrary.AbsoluteIdentifier.Substring(0, 13))
                {
                    //propName = propName.TrimEnd("Id".ToCharArray());
                    var targetFilter = GetOrCreateFilterObject(propName);
                    var targetType = new GenericObjectInterface(_schemaBuilder.Schema, _schemaBuilder.LanguagesType);
                    foreach (var targetProp in targetType.Fields)
                    {
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_not" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_in" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_not_in" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_lt" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_lte" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_gt" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_gte" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_contains" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_not_contains" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_starts_with" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_not_starts_with" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_ends_with" });
                        targetFilter.Filter.AddField(new FieldType { ResolvedType = targetProp.ResolvedType, Type = targetProp.Type, Name = $"{targetProp.Name}_not_ends_with" });
                    }
                    targetFilter.IsInitialized = true;
                    filter.AddField(new FieldType
                    {
                        Name = propName,
                        Type = typeof(InputObjectGraphType<object>),
                        ResolvedType = targetFilter.Filter
                    });
                }
                else
                {
                    var graphType = prop.NativeType.GetGraphTypeFromType(true);
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

                    enumOrderBy.AddValue($"{propName}_ASC", $"Order by {prop.Name} ascending", new Tuple<string, int>(prop.Owner.GetPropertyDescription(propName).Id, 1));
                    enumOrderBy.AddValue($"{propName}_DESC", $"Order by {prop.Name} descending", new Tuple<string, int>(prop.Owner.GetPropertyDescription(propName).Id, -1));
                }
            }

            foreach (var rel in clazz.Relationships)
            {
                var targetName = rel.Path.Last().TargetSchemaName;
                var targetFilter = GetOrCreateFilterObject(targetName);
                filter.AddField(new FieldType
                {
                    Name = rel.Name + "_some",
                    Type = typeof(ListGraphType<NonNullGraphType<InputObjectGraphType<object>>>),
                    ResolvedType = new ListGraphType(new NonNullGraphType(targetFilter.Filter))
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

        private FilterItem GetOrCreateFilterObject(string className)
        {
            var filterName = className + "Filter";
            var orderByName = className + "OrderBy";
            if (!Filters.TryGetValue(filterName, out var filter))
            {
                filter = new FilterItem
                {
                    Filter = new InputObjectGraphType<object> { Name = filterName },
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
                    //if (propertyId != MetaAttributeLibrary.AbsoluteIdentifier.Substring(0, 13))
                    //{
                        
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lt"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_lte"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gt"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_gte"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_contains"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_contains"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_starts_with"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_starts_with"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_ends_with"});
                    //    filter.AddField(new FieldType {ResolvedType = resolvedType, Type = graphType, Name = $"{propertyName}_not_ends_with"});
                    //}
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
        }
    }
}
