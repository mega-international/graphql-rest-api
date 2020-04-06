using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;

using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.Filters;
using Hopex.Modules.GraphQL.Schema.Formats;
using Hopex.Modules.GraphQL.Schema.Types;

using Mega.Macro.API;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mega.Macro.API.Library;
using Hopex.Model.DataModel;

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
        private Dictionary<string, IGraphType> _enums { get; } = new Dictionary<string, IGraphType>();

        public global::GraphQL.Types.Schema Schema { get; internal set; }
        public IHopexMetaModel HopexSchema { get; internal set; }

        private HopexEnumerationGraphType _languagesType;

        private readonly GraphQLSchemaManager _schemaManager;

        
        private ILogger Logger { get; }

        public SchemaBuilder(IHopexMetaModel hopexSchema, Dictionary<string, string> languages, ILogger logger, GraphQLSchemaManager schemaManager)
        {
            HopexSchema = hopexSchema;
            Logger = logger;
            _schemaManager = schemaManager;
            _languagesType = new LanguagesEnumerationGraphType(languages, ToValidName);
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
                DirectiveGraphType.Skip
            };
            return Schema;
        }

        internal IGraphType GetOrCreateEnumType(IPropertyDescription prop)
        {
            var enumTypeName = string.Concat(prop.Owner.Name, prop.Name, "Enum");
            if(_enums.TryGetValue(enumTypeName, out var e)) {
                return e;
            }

            var enumType = new HopexEnumerationGraphType
            {
                Name = enumTypeName
            };
            foreach (var ev in prop.EnumValues)
            {
                enumType.AddValue(ToValidName(ev.Name), ev.Description ?? ev.Name, ev.InternalValue);
            }
            Schema.RegisterType(enumType);
            _enums[enumTypeName] = enumType;
            return enumType;
        }

        private IObjectGraphType CreateQuerySchema(global::GraphQL.Types.Schema schema, IHopexMetaModel hopexSchema)
        {
            var genericObjectInterface = new GenericObjectInterface(schema, _languagesType);                            

            var query = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Query"
            };
            
            query.Field<ContextType>("_currentContext", resolve: context => new ContextType());
            query.Field<DiagnosticType>("_APIdiagnostic", resolve: context => new DiagnosticType());
            var builder = new FilterArgumentBuilder(this);

            // First generate classes
            foreach (var entity in hopexSchema.Classes)
            {
                var type = new ObjectGraphType<IModelElement>
                {
                    Description = entity.Description,
                    Name = entity.Name
                };
                _types.Add(entity.Id, new TypeInfo { GraphType = type, Description = entity });

                if (string.Compare(hopexSchema.Name, "metamodel", true) == 0)
                {
                    type.Field<ListGraphType<SchemaGraphType>>(
                        "schemas",
                        "Included in schemas",
                        resolve: ctx => ResolveSchemas(ctx.Source, entity)
                    );
                }

                genericObjectInterface.ImplementInConcreteType(entity.Id, type);

                CreateSpecificFields(entity, type);

                // Add arguments

                CreateProperties<IModelElement>(entity.Properties, type, (ctx, prop) =>
                {
                    string format = null;
                    if(ctx.Arguments != null && ctx.Arguments.ContainsKey("format"))
                    {
                         format = ctx.Arguments["format"].ToString();
                    }
                    var result = ctx.Source.GetValue<object>(prop, ctx.Arguments, format);
                    if (result is DateTime date && !string.IsNullOrEmpty(format))
                    {
                        return date.ToString(format);
                    }
                    return result;
                });

                if (entity.IsEntryPoint)
                {
                    var arguments = builder.BuildFilterArguments(entity, true);
                    query.Field<ListGraphType<ObjectGraphType<IModelElement>>>(
                        entity.Name,
                        entity.Description,
                        arguments,
                        (ctx) => GetElements(((UserContext)ctx.UserContext).IRoot, ctx.Arguments, ctx.Source, entity, true)
                    )
                    .ResolvedType = new ListGraphType(type);
                }
            }

            // Then generate relationships
            GenerateRelationships(hopexSchema, builder, true);
            // Si target est <> (voir schemapivotconvertor line 167)
            // alors générer un nouveau type qui hérite du précédent
            // Du coup on le fait dans une dernière phase pour étre certain de récupérer les relations éventuelles
            // qui vont être crées dans l'étape précédente
            GenerateRelationships(hopexSchema, builder, false);
            return query;
        }

        private IEnumerable<SchemaType> ResolveSchemas(IModelElement elem, IClassDescription metaClass)
        {
            var list = new List<SchemaType>();
            try
            {
                var metaClassId = NormalizeHopexId(elem.GetValue<string>("id"));
                if (!string.IsNullOrWhiteSpace(metaClassId))
                {                   
                    foreach (var sb in _schemaManager.HopexSchemas)
                    {
                        if (string.Compare(sb.Name, "metamodel", true) != 0)
                        {
                            var t = sb.FindClassDescriptionById(metaClassId);
                            if (t != null)
                            {
                                list.Add(new SchemaType { Name = sb.Name });
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return list;
        }

        // Valider si c'est nécessaire sinon à virer
        private static string NormalizeHopexId(string id)
        {
            if (id != null && id.ToString().Length > 1 && id.ToString()[0] != '~')
            {
                id = '~' + id.ToString();
            }
            return id;
        }

        const string ID_BUSINESS_DOCUMENT = "~UkPT)TNyFDK5";
        const string ID_SYSTEM_BUSINESS_DOCUMENT = "~aMRn)bUIGjX3";
        const string ID_BUSINESS_DOCUMENT_VERSION = "~AbEujCE8GfyD";
        const string ID_SYSTEM_BUSINESS_DOCUMENT_VERSION = "~3MRn64VIGb64";
        const string ID_DIAGRAM = "~KekPBSs3iS10";
        const string ID_SYSTEM_DIAGRAM = "~U20000000w10";

        private void CreateSpecificFields(IClassDescription entity, ObjectGraphType<IModelElement> type)
        {
            var hasUploadUrl = entity.Id == ID_BUSINESS_DOCUMENT || entity.Id == ID_SYSTEM_BUSINESS_DOCUMENT;
            if (hasUploadUrl)
                type.Field(
                    typeof(StringGraphType),
                    "uploadUrl",
                    "Upload document from here",
                    resolve: ctx => $"{((UserContext)ctx.UserContext).WebServiceUrl}/api/attachment/{ctx.Source.Id}/file");

            var hasDownloadUrl = hasUploadUrl || entity.Id == ID_BUSINESS_DOCUMENT_VERSION || entity.Id == ID_SYSTEM_BUSINESS_DOCUMENT_VERSION;
            if (hasDownloadUrl)
                type.Field(
                    typeof(StringGraphType),
                    "downloadUrl",
                    "Download document from here",
                    resolve: ctx => $"{((UserContext)ctx.UserContext).WebServiceUrl}/api/attachment/{ctx.Source.Id}/file");

            var hasDiagramImageUrl = entity.Id == ID_DIAGRAM || entity.Id == ID_SYSTEM_DIAGRAM;
            if (hasDiagramImageUrl)
                type.Field(
                    typeof(StringGraphType),
                    "downloadUrl",
                    "Download diagram image from here",
                    resolve: ctx => $"{((UserContext)ctx.UserContext).WebServiceUrl}/api/diagram/{ctx.Source.Id}/image");
        }

        static private readonly Dictionary<string, object> NO_ARGUMENTS = new Dictionary<string, object>();

        private void GenerateRelationships(IHopexMetaModel hopexSchema, FilterArgumentBuilder builder, bool firstPass)
        {
            foreach (var entity in hopexSchema.Classes)
            {
                var type = _types[entity.Id].GraphType;

                foreach (var link in entity.Relationships)
                {
                    var targetType = _types[link.Path.Last().TargetSchemaId];
                    var graphType = targetType.GraphType;
                    if (firstPass && targetType.Description == link.TargetClass)
                    {
                        CreateRelationship(builder, link, type, targetType.Description, graphType);
                        continue;
                    }

                    if (!firstPass && targetType.Description != link.TargetClass)
                    {
                        var tc = new ObjectGraphType<IModelElement>
                        {
                            Description = link.TargetClass.Description,
                            Name = link.TargetClass.Name
                        };
                        CreateProperties<IModelElement>(link.TargetClass.Properties, tc, (ctx, prop) =>
                        {
                            string format = null;
                            if(ctx.Arguments != null && ctx.Arguments.ContainsKey("format"))
                            {
                                 format = ctx.Arguments["format"].ToString();
                            }
                            var result = ctx.Source.GetValue<object>(prop, ctx.Arguments, format);
                            if (result is DateTime date && !string.IsNullOrEmpty(format))
                            {
                                return date.ToString(format);
                            }
                            return result;
                        });
                        CreateRelationship(builder, link, type, link.TargetClass, tc);
                    }
                }
            }
        }

        private void CreateRelationship(FilterArgumentBuilder builder, IRelationshipDescription link, ObjectGraphType<IModelElement> type, IClassDescription entity, IGraphType targetClassType)
        {
            //Do not generate filter for Diagram_Description
            var arguments = new QueryArguments();
            if (link.Id != "EIedE)fyA1y0")
            {
                arguments = builder.BuildFilterArguments(link.TargetClass, false);
            }

            type.FieldAsync<ListGraphType<ObjectGraphType<IModelElement>>>(
                link.Name,
                link.Description,
                arguments,
                async (ctx) => await GetElements(((UserContext)ctx.UserContext).IRoot, ctx.Arguments ?? NO_ARGUMENTS, ctx.Source, entity, false, link.Name)
            )
            .ResolvedType = new ListGraphType(targetClassType);
        }

        private IObjectGraphType CreateMutationSchema(global::GraphQL.Types.Schema graphQLSchema, IHopexMetaModel hopexSchema)
        {
            var mutation = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Mutation"
            };

            var mutationListFactory = new MutationListTypeFactory(this, graphQLSchema, hopexSchema);

            // Two phases
            // First generate input
            foreach (var entity in hopexSchema.Classes)
            {               
                var type = new InputObjectGraphType<object>
                {
                    Description = $"Input type for {entity.Name}",
                    Name = "Input" + entity.Name
                };

                // Add arguments
                CreateProperties<IModelElement>(entity.Properties.Where(p => !p.IsReadOnly), type, (ctx, prop) =>
                {
                    string format = null;
                    if(ctx.Arguments != null && ctx.Arguments.ContainsKey("format"))
                    {
                         format = ctx.Arguments["format"].ToString();
                    }
                    var val = ctx.GetArgument<object>(prop.Name, format);
                    var elem = ctx.Source;
                    elem.SetValue(prop, val, format);
                    return val;
                });

                // Then generate relationships
                foreach (var link in entity.Relationships)
                {
                    var listType = mutationListFactory.CreateMutationList(link.TargetClass);
                    var field = new FieldType
                    {
                        ResolvedType = listType,
                        Name = link.Name,
                        Description = link.Description,
                    };
                    type.AddField(field);
                }

                InputCustomPropertyType.AddCustomFields(type);
                InputCustomRelationshipType.AddCustomRelations(type);                

                var enumCreationMode = new HopexEnumerationGraphType { Name = "creationMode" };
                enumCreationMode.AddValue("RAW", "Do not use InstanceCreator, but classic \"create mode\"", false);
                enumCreationMode.AddValue("BUSINESS", "Use InstanceCreator", true);
                var queryArgumentsCreate = new QueryArguments(
                    new QueryArgument(typeof(StringGraphType)) { Name = "id" },
                    new QueryArgument(typeof(IdType)) { Name = "idType" },
                    new QueryArgument(new NonNullGraphType(type)) { Name = entity.Name.ToCamelCase() });
                if (!Equals(entity.Id, "~UkPT)TNyFDK5"))
                {
                    queryArgumentsCreate.Add(new QueryArgument(enumCreationMode) { Name = "creationMode" });
                }

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Create" + entity.Name,
                    $"Create a {entity.Name}",
                    queryArgumentsCreate,
                    async (ctx) =>
                    {
                        var useInstanceCreator = false;
                        if (ctx.Arguments != null && ctx.Arguments.ContainsKey("creationMode"))
                        {
                            useInstanceCreator = (bool)ctx.Arguments["creationMode"];
                        }
                        string id = null;
                        if(ctx.Arguments != null && ctx.Arguments.ContainsKey("id"))
                        {
                            id = ctx.Arguments["id"].ToString();
                        }
                        var idType = IdTypeEnum.INTERNAL;
                        if (ctx.Arguments != null && ctx.Arguments.ContainsKey("idType"))
                        {
                            Enum.TryParse(ctx.Arguments["idType"].ToString(), out idType);
                        }
                        return await ctx.Source.CreateElementAsync(
                            entity,
                            id,
                            idType,
                            useInstanceCreator,
                            CreateSetters(entity, ctx));
                    })
                .ResolvedType = new NonNullGraphType(_types[entity.Id].GraphType);

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Update" + entity.Name,
                    $"Update a {entity.Name}",
                    new QueryArguments(
                       new QueryArgument(typeof(NonNullGraphType<StringGraphType>)) { Name = "id" },
                       new QueryArgument(typeof(IdType)) { Name = "idType" },
                       new QueryArgument(new NonNullGraphType(type)) { Name = entity.Name.ToCamelCase() }
                    ),
                    async (ctx) =>
                    {
                        string id = null;
                        if(ctx.Arguments != null && ctx.Arguments.ContainsKey("id"))
                        {
                            id = ctx.Arguments["id"].ToString();
                        }
                        var idType = IdTypeEnum.INTERNAL;
                        if (ctx.Arguments != null && ctx.Arguments.ContainsKey("idType"))
                        {
                            Enum.TryParse(ctx.Arguments["idType"].ToString(), out idType);
                        }
                        return await ctx.Source.UpdateElementAsync(
                            entity,
                            id,
                            idType,
                            CreateSetters(entity, ctx));
                    })
                .ResolvedType = new NonNullGraphType(_types[entity.Id].GraphType);

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "CreateUpdate" + entity.Name,
                    $"Create or update a {entity.Name}",
                    queryArgumentsCreate,
                    async (ctx) =>
                    {
                        var useInstanceCreator = false;
                        if (ctx.Arguments.ContainsKey("creationMode"))
                        {
                            useInstanceCreator = (bool)ctx.Arguments["creationMode"];
                        }
                        string id = null;
                        if(ctx.Arguments != null && ctx.Arguments.ContainsKey("id"))
                        {
                            id = ctx.Arguments["id"].ToString();
                        }
                        var idType = IdTypeEnum.INTERNAL;
                        if (ctx.Arguments.ContainsKey("idType"))
                        {
                            Enum.TryParse(ctx.Arguments["idType"].ToString(), out idType);
                        }
                        return await ctx.Source.CreateUpdateElementAsync(
                            entity,
                            id,
                            idType,
                            CreateSetters(entity, ctx),
                            useInstanceCreator);
                    })
                .ResolvedType = new NonNullGraphType(_types[entity.Id].GraphType);

                mutation.FieldAsync<ObjectGraphType<object>>(
                    "Delete" + entity.Name,
                    $"Delete a {entity.Name}",
                    new QueryArguments(
                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" },
                       new QueryArgument(typeof(IdType)) { Name = "idType" },
                       new QueryArgument<BooleanGraphType> { Name = "cascade" }
                    ),
                    async (ctx) =>
                    {
                        string id = null;
                        if(ctx.Arguments != null && ctx.Arguments.ContainsKey("id"))
                        {
                            id = ctx.Arguments["id"].ToString();
                        }
                        var idType = IdTypeEnum.INTERNAL;
                        if (ctx.Arguments.ContainsKey("idType"))
                        {
                            Enum.TryParse(ctx.Arguments["idType"].ToString(), out idType);
                        }
                        var isCascade = false;
                        if (ctx.HasArgument("cascade"))
                        {
                            isCascade = (bool)ctx.Arguments["cascade"];
                        }
                        return await ctx.Source.RemoveElementAsync(
                            entity,
                            id,
                            idType,
                            isCascade);
                    })
                .ResolvedType = _types[entity.Id].GraphType;
            }

            return mutation;
        }

        internal void CreateProperties<T>(IEnumerable<IPropertyDescription> properties, IComplexGraphType type, Func<ResolveFieldContext<T>, IPropertyDescription, object> resolver)
        {
            foreach (var prop in properties)
            {
                var propertyName = ToValidName(prop.Name);
                var propertyType = prop.NativeType.GetGraphTypeFromType(true);

                IGraphType resolvedType = null;
                if (prop.EnumValues?.Count() > 0)
                {
                    propertyType = null;
                    resolvedType = GetOrCreateEnumType(prop);
                }

                var field = new FieldType
                {
                    Type = propertyType,
                    ResolvedType = resolvedType,
                    Name = propertyName,
                    Description = prop.Description,
                    Resolver = new FuncFieldResolver<T, object>(ctx => resolver(ctx, prop)),
                    Arguments = new QueryArguments()
                };

                if (prop.IsFormattedText)
                {
                    field.Arguments.Add(new QueryArgument(typeof(StringFormat)) { Name = "format" });
                }
                if (prop.IsTranslatable)
                {
                    field.Arguments.Add(new QueryArgument(_languagesType) { Name = "language" });
                }
                if (prop.Id == MetaAttributeLibrary.ShortName.Remove(13))
                {
                    field.Arguments.Add(new QueryArgument(typeof(NameSpaceFormat)) { Name = "nameSpace" });
                }
                switch (prop.PropertyType)
                {
                    case PropertyType.Date:
                        field.Arguments.Add(new QueryArgument(typeof(StringGraphType)) { Name = "format" });
                        break;
                    case PropertyType.Currency:
                        field.Arguments.Add(new QueryArgument(typeof(StringGraphType)) { Name = "currency" });
                        field.Arguments.Add(new QueryArgument(typeof(DateGraphType)) { Name = "dateRate" });
                        break;
                }

                type.AddField(field);
            }
        }

        private IEnumerable<ISetter> CreateSetters(IClassDescription entity, ResolveFieldContext<IHopexDataModel> ctx = null)
        {
            var arguments = (Dictionary<string, object>)ctx.Arguments[entity.Name.ToCamelCase()];
            foreach (var kv in arguments)
            {
                if (InputCustomPropertyType.IsCustomFieldsArgument(kv))
                {
                    foreach (var customPropertySetter in CustomFieldSetter.CreateSetters(kv.Value))
                        yield return customPropertySetter;
                }
                else if (InputCustomRelationshipType.IsCustomRelationsArgument(kv))
                {
                    foreach (var customRelationSetter in CustomRelationshipSetter.CreateSetters(kv.Value))
                        yield return customRelationSetter;
                }
                else
                {
                    var prop = entity.GetPropertyDescription(kv.Key, false);
                    if (prop != null)
                    {
                        yield return PropertySetter.Create(prop, kv.Value);
                    }
                    else
                    {
                        var rel = entity.GetRelationshipDescription(kv.Key, false);
                        if (rel != null)
                        {
                            if (kv.Value is Dictionary<string, object> dict)
                            {
                                var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), dict["action"].ToString(), true);
                                var list = (List<object>)dict["list"];
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
        }

        private async Task<IEnumerable<IModelElement>> EnumerateRootElements(IMegaRoot root, Dictionary<string, object> arguments, IHasCollection source, IClassDescription entity, string relationshipName, bool isRoot)
        {
            string erql = null;
            var list = new List<IModelElement>();
            var sourceElement = source as IModelElement;
            IRelationshipDescription relationship = null;
            if (sourceElement != null)
            {
                relationship = sourceElement.ClassDescription.GetRelationshipDescription(relationshipName);
            }
            if (arguments.TryGetValue("filter", out var obj) && obj is Dictionary<string, object> filter)
            {
                var compiler = new FilterCompiler(entity, relationship, isRoot ? null : sourceElement);
                erql = $"SELECT {entity.Id}[{entity.GetBaseName()}]";
                erql += " WHERE " + compiler.CreateHopexQuery(filter);
                Logger.LogInformation($"{erql}");
                if (root is ISupportsDiagnostics diags)
                {
                    diags.AddGeneratedERQL(erql);
                }
            }

            var orderByClauses = new List<Tuple<string, int>>();
            if (arguments.TryGetValue("orderBy", out obj))
            {
                var orderBy = (List<object>)obj;
                foreach (Tuple<string, int> o in orderBy)
                {
                    orderByClauses.Add(o);
                }
            }

            var collection = await source.GetCollectionAsync(entity.Name, erql, orderByClauses, relationshipName);

            var orderedCollection = collection.ToList();
            foreach (var orderByClause in orderByClauses)
            {
                if (orderByClause.Item2 == 1)
                {
                    orderedCollection = orderedCollection.OrderBy(x => x.MegaObject.GetPropertyValue(orderByClause.Item1)).ToList();
                }
                else if (orderByClause.Item2 == -1)
                {
                    orderedCollection = orderedCollection.OrderByDescending(x => x.MegaObject.GetPropertyValue(orderByClause.Item1)).ToList();
                }
            }
            return GetPaginatedModelElements(orderedCollection, arguments);
        }

        public static IEnumerable<IModelElement> GetPaginatedModelElements(IReadOnlyList<IModelElement> orderedCollection, IReadOnlyDictionary<string, object> arguments)
        {
            object obj;
            int? first = null;
            var after = "";
            int? last = null;
            var before = "";
            var skip = 0;

            if (arguments.TryGetValue("first", out obj))
            {
                first = (int)obj;
            }
            if (arguments.TryGetValue("after", out obj))
            {
                after = (string)obj;
            }
            if (arguments.TryGetValue("last", out obj))
            {
                last = (int)obj;
            }
            if (arguments.TryGetValue("before", out obj))
            {
                before = (string)obj;
            }
            if (arguments.TryGetValue("skip", out obj))
            {
                skip = (int)obj;
            }



            var list = new List<IModelElement>();

            if (last > 0 && first == null)
            {
                var isIgnore = !string.IsNullOrEmpty(before);
                for (var i = orderedCollection.Count - 1; i >= 0 && last > 0; i--)
                {
                    if (!string.IsNullOrEmpty(after) && (string)orderedCollection[i].Id.Value == after)
                    {
                        break;
                    }
                    if (isIgnore)
                    {
                        if ((string)orderedCollection[i].Id.Value == before)
                        {
                            isIgnore = false;
                        }
                        continue;
                    }
                    var permissions = orderedCollection[i].GetCrud();
                    if (permissions.IsReadable && !orderedCollection[i].IsConfidential && orderedCollection[i].IsAvailable)
                    {
                        if (skip-- > 0)
                        {
                            continue;
                        }
                        list.Add(orderedCollection[i]);
                        last--;
                    }
                }

                list.Reverse();
            }
            else
            {
                if (first == null)
                {
                    first = int.MaxValue;
                }
                var isIgnore = !string.IsNullOrEmpty(after);
                for (var i = 0; i < orderedCollection.Count && first > 0; i++)
                {
                    if (!string.IsNullOrEmpty(before) && (string)orderedCollection[i].Id.Value == before)
                    {
                        break;
                    }
                    if (isIgnore)
                    {
                        if ((string)orderedCollection[i].Id.Value == after)
                        {
                            isIgnore = false;
                        }
                        continue;
                    }
                    var permissions = orderedCollection[i].GetCrud();
                    if (permissions.IsReadable && !orderedCollection[i].IsConfidential && orderedCollection[i].IsAvailable)
                    {
                        if (skip-- > 0)
                        {
                            continue;
                        }
                        list.Add(orderedCollection[i]);
                        first--;
                    }
                }
            }

            return list;
        }

        private async Task<IEnumerable<IModelElement>> GetElements(IMegaRoot iRoot, Dictionary<string, object> arguments, IHasCollection source, IClassDescription entity, bool isRoot, string linkName = null)
        {
            var permissions = CrudComputer.GetCollectionMetaPermission(iRoot, entity.Id);
            if (!permissions.IsReadable)
                return Enumerable.Empty<IModelElement>();

            var megaCollection = await EnumerateRootElements(iRoot, arguments, source, entity, linkName, isRoot);

            return megaCollection;
        }

        private static string ToValidName(string val)
        {
            val = RemoveDiacritics(val);
            var pattern = @"[^a-zA-Z0-9_]";
            var regex = new Regex(pattern);
            val = regex.Replace(val, "_");
            if (val.Length > 0 && !char.IsLetter(val[0]))
            {
                return "E" + val;
            }
            return val;
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
