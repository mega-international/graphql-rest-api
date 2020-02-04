using GraphQL;
using GraphQL.Types;

using Hopex.Model.Abstractions.MetaModel;
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
            public bool IsInitialized;
        }

        internal Dictionary<string, FilterItem> Filters { get; } = new Dictionary<string, FilterItem>();

        public QueryArguments BuildFilterArguments(IClassDescription clazz, bool fromRoot)
        {
            var arguments = new QueryArguments();
            string filterName = clazz.Name + "Filter";
            var filterItem = GetOrCreateFilterObject(filterName);
            arguments.Add(new QueryArgument(filterItem.Filter) { Name = "filter" });
            arguments.Add(new QueryArgument<IntGraphType> { Name = "skip" });
            arguments.Add(new QueryArgument<IntGraphType> { Name = "take" });
            
            if (filterItem.IsInitialized)
            {
                return arguments;
            }

            filterItem.IsInitialized = true;
            var filter = filterItem.Filter;
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

            if (fromRoot)
            {
                filter.AddField(new FieldType { Type = typeof(StringGraphType), Name = "id" });
            }
            filter.AddField(new FieldType { Type = typeof(StringGraphType),Name = "id_not"});
            filter.AddField(new FieldType { Type = typeof(ListGraphType<NonNullGraphType<StringGraphType>>), Name ="id_in"});
            filter.AddField(new FieldType { Type = typeof(ListGraphType<NonNullGraphType<StringGraphType>>), Name ="id_not_in"});

            var enumOrderBy = new HopexEnumerationGraphType
            {
                Name = $"orderBy{clazz.Name}"
            };

            foreach (var prop in clazz.Properties)
            {
                var graphType = prop.NativeType.GetGraphTypeFromType(true);
                IGraphType resolvedType = null;
                if(prop.EnumValues?.Count() > 0)
                {
                    var enumTypeName = string.Concat(clazz.Name, prop.Name, "Enum");
                    resolvedType = _schemaBuilder.Enums[enumTypeName];
                }

                var nonNullType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
                var typeList = typeof(ListGraphType<>).MakeGenericType(nonNullType);
                var propName = prop.Name.ToCamelCase();
                
                filter.AddField(new FieldType { ResolvedType=resolvedType, Type = graphType, Name = $"{propName}" });
                filter.AddField(new FieldType { ResolvedType=resolvedType, Type = graphType,Name = $"{propName}_not"});

                if (graphType != typeof(BooleanGraphType))
                {
                    if(resolvedType != null)
                    {
                        resolvedType = new ListGraphType(new NonNullGraphType(resolvedType));
                    }
                    filter.AddField(new FieldType { ResolvedType=resolvedType, Type = typeList, Name =$"{propName}_in"});
                    filter.AddField(new FieldType { ResolvedType=resolvedType, Type = typeList, Name =$"{propName}_not_in"});

                    if (resolvedType == null) // Enum
                    {
                        filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_lt"});
                        filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_lte"});
                        filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_gt"});
                        filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_gte"});

                        if (graphType == typeof(StringGraphType))
                        {
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name =  $"{propName}_contains"});
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_not_contains"});
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_starts_with"});
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_not_starts_with"});
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_ends_with"});
                            filter.AddField(new FieldType { ResolvedType=null, Type = graphType,Name = $"{propName}_not_ends_with"});
                        }
                    }
                }

                enumOrderBy.AddValue($"{propName}_ASC", $"Order by {prop.Name} ascending", new Tuple<string , int>(prop.ClassDescription.GetPropertyDescription(propName).Id.ToString(), 1));
                enumOrderBy.AddValue($"{propName}_DESC", $"Order by {prop.Name} descending", new Tuple<string , int>(prop.ClassDescription.GetPropertyDescription(propName).Id.ToString(), -1));
            }

            foreach (var rel in clazz.Relationships)
            {
                var targetName = rel.Path.Last().TargetSchemaName;
                var targetFilter = GetOrCreateFilterObject(targetName + "Filter");
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

            if (enumOrderBy.Values.Any())
            {
                arguments.Add(new QueryArgument(new ListGraphType(enumOrderBy)) { Name = "orderBy" });
            }

            return arguments;
        }

        private FilterItem GetOrCreateFilterObject(string filterName)
        {            
            if (!Filters.TryGetValue(filterName, out var filter))
            {
                filter = new FilterItem { Filter = new InputObjectGraphType<object>() { Name = filterName }, IsInitialized = false };
                Filters.Add(filterName, filter);
            }
            return filter;
        }
    }
}
