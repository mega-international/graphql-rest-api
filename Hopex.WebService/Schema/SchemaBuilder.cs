using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Hopex.Modules.GraphQL.Schema.Directives;
using Hopex.Modules.GraphQL.Schema.Filters;
using Hopex.Modules.GraphQL.Schema.Formats;
using Mega.Macro.API;

namespace Hopex.Modules.GraphQL.Schema
{
    internal class SchemaBuilder
    {
        private struct TypeInfo
        {
            public ObjectGraphType<IModelElement> GraphType { get; set; }
            public IClassDescription Description { get; set; }
        }

        private readonly Dictionary<MegaId, TypeInfo> _types = new Dictionary<MegaId, TypeInfo>();

        public global::GraphQL.Types.Schema Schema { get; internal set; }
        public IHopexMetaModel HopexSchema { get; internal set; }

        public SchemaBuilder(IHopexMetaModel hopexSchema)
        {
            HopexSchema = hopexSchema;
        }

        public global::GraphQL.Types.Schema Create()
        {
            Schema = new global::GraphQL.Types.Schema();
            Schema.Query = CreateQuerySchema(Schema, HopexSchema);
            Schema.Mutation = CreateMutationSchema(Schema, HopexSchema);
            Schema.Directives = new List<DirectiveGraphType>
            {
                DirectiveGraphType.Deprecated,
                DirectiveGraphType.Include,
                FormatDirective.Instance
            };
            return Schema;
        }

        private IObjectGraphType CreateQuerySchema(global::GraphQL.Types.Schema schema, IHopexMetaModel hopexSchema)
        {
            var query = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Query"
            };

            // First generate classes
            foreach(var entity in hopexSchema.Classes)
            {
                var type = new ObjectGraphType<IModelElement>
                {
                    Description = entity.Description,
                    Name = entity.Name
                };
                _types.Add(entity.Id, new TypeInfo { GraphType = type, Description = entity });

                type.Field(typeof(StringGraphType), "Id", "Absolute identifier", resolve: ctx => ctx.Source.Id.ToString());

                // Add arguments
                CreateProperties<IModelElement>(schema, entity.Properties, type, (ctx, prop) =>
                {
                    var format = ctx.FieldAst.Arguments.ValueFor("format")?.Value.ToString();
                    return ctx.Source.GetValue<object>(prop, format);
                });

                if(entity.IsEntryPoint)
                {
                    query.Field<ListGraphType<ObjectGraphType<IModelElement>>>(
                        entity.Name,
                        entity.Description,
                        CreateArgumentList(entity),
                        (ctx) => GetElements(ctx.Arguments, ctx.Source, entity)
                    )
                    .ResolvedType = new ListGraphType(type);
                }
            }

            // Then generate relationships
            foreach(var entity in hopexSchema.Classes)
            {
                var type = _types [entity.Id].GraphType;

                foreach(var link in entity.Relationships)
                {
                    var targetType = _types [link.Path.Last().TargetSchemaId];
                    type.FieldAsync<ListGraphType<ObjectGraphType<IModelElement>>>(
                        link.Name,
                        link.Description,
                        CreateArgumentList(targetType.Description),
                        async (ctx) => await GetElements(ctx.Arguments, ctx.Source, targetType.Description, link.Name)
                    )
                    .ResolvedType = new ListGraphType(targetType.GraphType);
                }
            }

            return query;
        }

        private IObjectGraphType CreateMutationSchema(global::GraphQL.Types.Schema schema, IHopexMetaModel hopexSchema)
        {
            var mutation = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Mutation"
            };

            var actionEnum = new EnumerationGraphType<CollectionAction> { Name = "_InputCollectionActionEnum" };
            schema.RegisterType(actionEnum);

            var inputListType = new InputObjectGraphType<Dictionary<string, object>> { Name = "_InputCollectionAction" };
            inputListType.AddField(new FieldType { Name = "action", ResolvedType = actionEnum });
            inputListType.Field<ListGraphType<NonNullGraphType<StringGraphType>>>("list");
            schema.RegisterType(inputListType);

            // Two phases
            // First generate input
            foreach(var entity in hopexSchema.Classes)
            {
                var type = new InputObjectGraphType<object>
                {
                    Description = $"Input type for {entity.Name}",
                    Name = "Input" + entity.Name
                };

                // Add arguments
                CreateProperties<IModelElement>(schema, entity.Properties.Where(p => !p.IsReadOnly), type, (ctx, prop) =>
                {
                    var format = ctx.FieldAst.Arguments.ValueFor("format")?.Value.ToString();
                    var val = ctx.GetArgument<object>(prop.Name, format);
                    var elem = ctx.Source;
                    elem.SetValue(prop, val, format);
                    return val;
                });

                // Then generate relationships
                foreach(var link in entity.Relationships)
                {
                    var field = new FieldType
                    {
                        ResolvedType = inputListType,
                        Name = link.Name,
                        Description = link.Description,
                    };
                    type.AddField(field);
                }

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Create" + entity.Name,
                    $"Create a {entity.Name}",
                    new QueryArguments(
                        new QueryArgument(new NonNullGraphType(type)) { Name = entity.Name.ToCamelCase() },
                        new QueryArgument<BooleanGraphType> { Name = "useInstanceCreator" }),
                    async (ctx) =>
                    {
                        var useInstanceCreator = false;
                        if (ctx.Arguments.ContainsKey("useInstanceCreator"))
                        {
                            useInstanceCreator = (bool) ctx.Arguments["useInstanceCreator"];
                        }
                        return await ctx.Source.CreateElementAsync(
                            entity,
                            CreateSetters(entity,(Dictionary<string, object>) ctx.Arguments[entity.Name.ToCamelCase()]),
                            useInstanceCreator);
                    })
                .ResolvedType = new NonNullGraphType(_types [entity.Id].GraphType);

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Update" + entity.Name,
                    $"Update a {entity.Name}",
                    new QueryArguments(
                       new QueryArgument(typeof(NonNullGraphType<StringGraphType>)) { Name = "id" },
                       new QueryArgument(new NonNullGraphType(type)) { Name = entity.Name.ToCamelCase() }
                    ),
                    async (ctx) => await ctx.Source.UpdateElementAsync(entity, ctx.Arguments ["id"]?.ToString(), CreateSetters(entity, (Dictionary<string, object>)ctx.Arguments [entity.Name.ToCamelCase()]))
                )
                .ResolvedType = new NonNullGraphType(_types [entity.Id].GraphType);

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Delete" + entity.Name,
                    $"Delete a {entity.Name}",
                    new QueryArguments(
                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" },
                       new QueryArgument<BooleanGraphType> { Name = "cascade" }
                    ),
                    async (ctx) => await ctx.Source.RemoveElementAsync(entity, ctx.Arguments ["id"]?.ToString(), (bool)ctx.Arguments ["cascade"])
                )
                .ResolvedType = _types [entity.Id].GraphType;
            }

            return mutation;
        }

        private void CreateProperties<T>(global::GraphQL.Types.Schema schema, IEnumerable<IPropertyDescription> properties, IComplexGraphType type, Func<ResolveFieldContext<T>, IPropertyDescription, object> resolver)
        {
            foreach(var prop in properties)
            {
                var propertyType = prop.NativeType.GetGraphTypeFromType(true);
                IGraphType resolvedType = null;
                if(prop.EnumValues?.Count() > 0)
                {
                    propertyType = null;
                    var enumType = new HopexEnumerationGraphType
                    {
                        Name = prop.Name + "Enum"
                    };
                    foreach(var e in prop.EnumValues)
                    {
                        enumType.AddValue(ToValidName(e.Name), e.Description ?? e.Name, e.InternalValue);
                    }
                    resolvedType = enumType;
                    schema.RegisterType(enumType);
                }

                var field = new FieldType
                {
                    Type = propertyType,
                    ResolvedType = resolvedType,
                    Name = ToValidName(prop.Name),
                    Description = prop.Description,
                    Resolver = new FuncFieldResolver<T, object>(ctx => resolver(ctx, prop)),
                    Arguments = new QueryArguments()
                };

                switch(field.Type?.Name)
                {
                    case "StringGraphType":
                        field.Arguments.Add(new QueryArgument(typeof(StringFormat)) { Name = "format" });
                        break;
                    case "DateGraphType":
                        field.Arguments.Add(new QueryArgument(typeof(StringGraphType)) { Name = "format" });
                        break;
                }

                type.AddField(field);
            }
        }

        private static QueryArguments CreateArgumentList(IClassDescription entity)
        {
            var filter = new InputObjectGraphType<object>
            {
                Description = $"Filters of {entity.Name}",
                Name = "filter" + entity.Name
            };

            var boolType = new BooleanGraphType();
            var intType = new IntGraphType();
            var doubleType = new FloatGraphType();
            var stringType = new StringGraphType();
            var dateType = new DateGraphType();
            var listIntType = new ListGraphType(intType);
            var listDoubleType = new ListGraphType(doubleType);
            var listStringType = new ListGraphType(stringType);
            var listDateType = new ListGraphType(dateType);

            if(entity.IsEntryPoint)
            {
                filter.AddField(new FieldType { Type = typeof(string).ToGraphType(false), ResolvedType = stringType, Name = "id" });
                filter.AddField(new FieldType { Type = typeof(string).ToGraphType(false), ResolvedType = stringType, Name = "id_not" });
                filter.AddField(new FieldType { Type = typeof(string).MakeArrayType().ToGraphType(false), ResolvedType = listStringType, Name = "id_in" });
                filter.AddField(new FieldType { Type = typeof(string).MakeArrayType().ToGraphType(false), ResolvedType = listStringType, Name = "id_not_in" });
            }

            foreach(var prop in entity.Properties)
            {
                var propertyName = char.ToLower(prop.Name [0]) + prop.Name.Substring(1);

                if(prop.IsFilterable)
                {
                    switch(prop.PropertyType)
                    {
                        case PropertyType.Boolean:
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = boolType, Name = propertyName });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = boolType, Name = propertyName + "_not" });
                            break;
                        case PropertyType.Int:
                        case PropertyType.Long:
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName + "_not" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listIntType, Name = propertyName + "_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listIntType, Name = propertyName + "_not_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName + "_lt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName + "_lte" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName + "_gt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = intType, Name = propertyName + "_gte" });
                            break;
                        case PropertyType.Double:
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName + "_not" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listDoubleType, Name = propertyName + "_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listDoubleType, Name = propertyName + "_not_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName + "_lt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName + "_lte" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName + "_gt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = doubleType, Name = propertyName + "_gte" });
                            break;
                        case PropertyType.String:
                        case PropertyType.RichText:
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_not" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listStringType, Name = propertyName + "_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listStringType, Name = propertyName + "_not_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_lt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_lte" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_gt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_gte" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_contains" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_not_contains" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_starts_with" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_not_starts_with" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_ends_with" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = stringType, Name = propertyName + "_not_ends_with" });
                            break;
                        case PropertyType.Date:
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName + "_not" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listDateType, Name = propertyName + "_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.MakeArrayType().ToGraphType(false), ResolvedType = listDateType, Name = propertyName + "_not_in" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName + "_lt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName + "_lte" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName + "_gt" });
                            filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = dateType, Name = propertyName + "_gte" });
                            break;
                        case PropertyType.Enum:
                            if(prop.EnumValues?.Count() > 0)
                            {
                                var enumType = new HopexEnumerationGraphType
                                {
                                    Name = prop.Name + "EnumFilter"
                                };
                                foreach(var e in prop.EnumValues)
                                {
                                    enumType.AddValue(ToValidName(e.Name), e.Description ?? e.Name, e.InternalValue);
                                }
                                filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = enumType, Name = propertyName });
                                filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = enumType, Name = propertyName + "_not" });
                                filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = enumType, Name = propertyName + "_in" });
                                filter.AddField(new FieldType { Type = prop.NativeType.ToGraphType(false), ResolvedType = enumType, Name = propertyName + "_not_in" });
                            }
                            break;
                    }
                }
            }
            return new QueryArguments { new QueryArgument(filter.GetNamedType()) { Name = "filter" } };
        }

        private IEnumerable<ISetter> CreateSetters(IClassDescription entity, Dictionary<string, object> arguments)
        {
            foreach(var kv in arguments)
            {
                var prop = entity.GetPropertyDescription(kv.Key, false);
                if(prop != null)
                {
                    yield return PropertySetter.Create(prop, kv.Value);
                }
                else
                {
                    var rel = entity.GetRelationshipDescription(kv.Key, false);
                    if(rel != null)
                    {
                        if(kv.Value is Dictionary<string, object> dict)
                        {
                            var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), dict ["action"].ToString(), true);
                            var list = (List<object>)dict ["list"];
                            yield return CollectionSetter.Create(rel, action, list);
                        }
                    }
                    else
                    {
                        throw new Exception($"{kv.Key} is not a valid member of {entity.Name}");
                    }
                }
            }
        }

        private async Task<IEnumerable<IModelElement>> GetElements(Dictionary<string, object> arguments, IHasCollection source, IClassDescription entity, string linkName = null)
        {
            var list = new List<IModelElement>();
            Dictionary<MegaId, FilterValue> filters = null;

            if(arguments.TryGetValue("filter", out var f))
            {
                if(f is Dictionary<string, object> filterArguments)
                {
                    filters = new Dictionary<MegaId, FilterValue>();
                    foreach(var argument in filterArguments)
                    {
                        var filterName = argument.Key;
                        var operation = GetOperation(filterName);
                        var propertyName = filterName;
                        if(!string.IsNullOrEmpty(operation))
                        {
                            propertyName = filterName.Substring(0, filterName.LastIndexOf(operation, StringComparison.Ordinal));
                        }
                        if(propertyName == "id")
                        {
                            if(operation == "" && source is IHopexDataModel dataModel)
                            {
                                var e = await dataModel.GetElementByIdAsync(entity, argument.Value.ToString());
                                if(e != null)
                                {
                                    list.Add(e);
                                }
                                return list;
                            }
                            var propertyDescription = new PropertyDescription(null, "Id", "310000000D00", "Absolute identifier", PropertyType.Id.ToString(), true, true, true);
                            filters.Add(propertyDescription.Id, FilterValue.Create(propertyDescription, operation, argument.Value));
                        }
                        else
                        {
                            var propertyDescription = entity.GetPropertyDescription(propertyName.ToCamelCase());
                            if(propertyDescription?.IsFilterable == true)
                            {
                                filters.Add(propertyDescription.Id, FilterValue.Create(propertyDescription, operation, argument.Value));
                            }
                        }
                    }
                }
            }

            Func<IModelElement, bool> filter = null;
            if(filters != null && filters.Count > 0)
            {
                filter = (e) =>
                {
                    foreach(var elementFilter in filters)
                    {
                        object val;
                        if(elementFilter.Value.PropertyDescription.Id.ToString() == "~310000000D00")
                        {
                            val = e.Id.ToString();
                        }
                        else
                        {
                            val = e.GetValue<object>(elementFilter.Value.PropertyDescription);
                        }
                        if(!elementFilter.Value.Compare(val))
                        {
                            return false;
                        }
                    }
                    return true;
                };
            }

            var megaCollection = source.GetCollectionAsync(linkName ?? entity.Name).GetAwaiter().GetResult();
            foreach(var element in megaCollection)
            {
                if(filter == null || filter(element))
                {
                    list.Add(element);
                }
                else
                {
                    (element as IDisposable)?.Dispose();
                }
            }

            return list;
        }

        private static string GetOperation(string filterName)
        {
            if(filterName.EndsWith("_not"))
            {
                return "_not";
            }
            if(filterName.EndsWith("_not_in"))
            {
                return "_not_in";
            }
            if(filterName.EndsWith("_in"))
            {
                return "_in";
            }
            if(filterName.EndsWith("_lt"))
            {
                return "_lt";
            }
            if(filterName.EndsWith("_lte"))
            {
                return "_lte";
            }
            if(filterName.EndsWith("_gt"))
            {
                return "_gt";
            }
            if(filterName.EndsWith("_gte"))
            {
                return "_gte";
            }
            if(filterName.EndsWith("_not_contains"))
            {
                return "_not_contains";
            }
            if(filterName.EndsWith("_contains"))
            {
                return "_contains";
            }
            if(filterName.EndsWith("_not_starts_with"))
            {
                return "_not_starts_with";
            }
            if(filterName.EndsWith("_starts_with"))
            {
                return "_starts_with";
            }
            if(filterName.EndsWith("_not_ends_with"))
            {
                return "_not_ends_with";
            }
            if(filterName.EndsWith("_ends_with"))
            {
                return "_ends_with";
            }
            return "";
        }

        private static string ToValidName(string val)
        {
            var pattern = @"[^a-zA-Z0-9_]";
            var regex = new Regex(pattern);
            val = regex.Replace(val, "_");
            if(!char.IsLetter(val [0]))
            {
                return "E" + val;
            }
            return val;
        }
    }
}
