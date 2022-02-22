using GraphQL;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Language.AST;
using GraphQL.Resolvers;
using GraphQL.Types;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.MetaModel;
using Hopex.Modules.GraphQL.Schema.Filters;
using Hopex.Modules.GraphQL.Schema.Formats;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using Hopex.Modules.GraphQL.Schema.Types;
using Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL.Schema
{
    internal class SchemaBuilder
    {
        private struct TypeInfo
        {
            public ObjectWithParentGraphType<IModelElement> GraphType { get; set; }
            public AggregationQueryType AggregationQueryType { get; set; }
            public GraphQLClassDescription GraphQlDescription { get; set; }
            public IClassDescription Description => GraphQlDescription.MetaClass;
        }

        private readonly Dictionary<MegaId, TypeInfo> _types = new Dictionary<MegaId, TypeInfo>();

        private Dictionary<string, IGraphType> Enums { get; } = new Dictionary<string, IGraphType>();

        public global::GraphQL.Types.Schema Schema { get; internal set; }
        public IHopexMetaModel HopexSchema { get; internal set; }
        public HopexEnumerationGraphType LanguagesType;
        public HopexEnumerationGraphType CurrenciesType;

        private readonly GraphQLSchemaManager _schemaManager;
        private readonly SchemaMappingResolver _schemaMappingResolver;
        private readonly FilterArgumentBuilder _filterArgumentBuilder;
        private readonly IClassDescription _genericClass;

        private readonly Dictionary<string, GraphQLClassDescription> _graphQlClasses = new Dictionary<string, GraphQLClassDescription>();

        private ILogger Logger { get; }

        public SchemaBuilder(IHopexMetaModel hopexSchema, Dictionary<string, IMegaObject> languages, List<string> currencies, ILogger logger, GraphQLSchemaManager schemaManager)
        {
            HopexSchema = hopexSchema;
            Logger = logger;
            _schemaManager = schemaManager;
            _schemaMappingResolver = new SchemaMappingResolver(_schemaManager);
            _filterArgumentBuilder = new FilterArgumentBuilder(this);
            _genericClass = hopexSchema.GetGenericClass();
            LanguagesType = new LanguagesEnumerationGraphType(languages, ToValidName);
            CurrenciesType = new CurrenciesEnumerationGraphType(currencies, ToValidName);
            Schema = new global::GraphQL.Types.Schema();
            Schema.FieldMiddleware.Use(new InstrumentFieldMiddleware());
            RegisterValueConverter();
        }

        private static void RegisterValueConverter()
        {
            ValueConverter.Register<short, sbyte>(value => Convert.ToSByte(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, byte>(value => Convert.ToByte(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, ushort>(value => Convert.ToUInt16(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register(typeof(short), typeof(bool), value => Convert.ToBoolean(value, NumberFormatInfo.InvariantInfo).Boxed());
            ValueConverter.Register<short, int>(value => Convert.ToInt32(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, uint>(value => Convert.ToUInt32(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, long>(value => value);
            ValueConverter.Register<short, ulong>(value => Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, BigInteger>(value => new BigInteger(value));
            ValueConverter.Register<short, double>(value => Convert.ToDouble(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, decimal>(value => Convert.ToDecimal(value, NumberFormatInfo.InvariantInfo));
            ValueConverter.Register<short, TimeSpan>(value => TimeSpan.FromSeconds(value));

            ValueConverter.Register<ushort, sbyte>(value => (sbyte)value);
            ValueConverter.Register<ushort, byte>(value => (byte)value);
            ValueConverter.Register<ushort, int>(value => value);
            ValueConverter.Register<ushort, uint>(value => value);
            ValueConverter.Register<ushort, long>(value => value);
            ValueConverter.Register<ushort, ulong>(value => value);
            ValueConverter.Register<ushort, short>(value => (short)value);
            ValueConverter.Register<ushort, BigInteger>(value => new BigInteger(value));
        }

        public global::GraphQL.Types.Schema Create(IMegaRoot megaRoot)
        {
            InitializeGraphQlClasses();
            Schema.Query = CreateQuerySchema(Schema, HopexSchema, megaRoot);
            Schema.Mutation = CreateMutationSchema(HopexSchema);

            Schema.Directives.Register(new DirectiveGraphType("capitalize", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("deburr", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("pascalCase", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("camelCase", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("kebabCase", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("snakeCase", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("toLower", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("lowerFirst", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("toUpper", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("upperFirst", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("trim", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("trimStart", new List<DirectiveLocation> { DirectiveLocation.Field }));
            Schema.Directives.Register(new DirectiveGraphType("trimEnd", new List<DirectiveLocation> { DirectiveLocation.Field }));

            Schema.Directives.Register(
                new DirectiveGraphType("phone", new List<DirectiveLocation> { DirectiveLocation.Field })
                {
                    Arguments = new QueryArguments
                    {
                        new QueryArgument(typeof(StringGraphType)) {Name = "format"}
                    }
                });
            Schema.Directives.Register(
                new DirectiveGraphType("number", new List<DirectiveLocation> { DirectiveLocation.Field })
                {
                    Arguments = new QueryArguments
                    {
                        new QueryArgument(typeof(StringGraphType)) {Name = "format"}
                    }
                });
            Schema.Directives.Register(new DirectiveGraphType("currency", new List<DirectiveLocation> { DirectiveLocation.Field })
            {
                Arguments = new QueryArguments
                    {
                        new QueryArgument(typeof(StringGraphType)) { Name = "format" },
                        new QueryArgument(typeof(StringGraphType)) { Name = "currency" }
                    }
            });
            Schema.Directives.Register(
                new DirectiveGraphType("date", new List<DirectiveLocation> { DirectiveLocation.Field })
                {
                    Arguments = new QueryArguments
                    {
                        new QueryArgument(typeof(StringGraphType)) {Name = "format"}
                    }
                });
            //Schema.Directives.Register(new DirectiveGraphType("isReadOnly", new List<DirectiveLocation> {DirectiveLocation.Field}));
            //Schema.Directives.Register(new DirectiveGraphType("isReadWrite", new List<DirectiveLocation> {DirectiveLocation.Field}));
            return Schema;
        }

        private void InitializeGraphQlClasses()
        {
            foreach(var metaclass in HopexSchema.Classes.Except(new List<IClassDescription>{_genericClass}))
            {
                _graphQlClasses.Add(metaclass.Name, new GraphQLClassDescription(metaclass));
            }

            foreach(var graphQlClasse in _graphQlClasses)
            {
                graphQlClasse.Value.GenerateBasicFields(_graphQlClasses);
            }

            foreach(var graphQlClasse in _graphQlClasses)
            {
                graphQlClasse.Value.GenerateContextedFields();
            }
        }

        internal IGraphType GetOrCreateEnumType(IPropertyDescription prop)
        {
            var enumTypeName = string.Concat(prop.Name, "Enum");
            if (Enums.TryGetValue(enumTypeName, out var e))
            {
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
            Enums[enumTypeName] = enumType;
            return enumType;
        }

        private IObjectGraphType CreateQuerySchema(global::GraphQL.Types.Schema schema, IHopexMetaModel hopexSchema, IMegaRoot megaRoot)
        {
            var genericObjectInterface = new GenericObjectInterface(schema, LanguagesType, _genericClass);

            var query = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Query"
            };

            query.Field<CurrentContextType>("_currentContext", resolve: context => new CurrentContextType());
            query.Field<DiagnosticType>("_APIdiagnostic", resolve: context => new DiagnosticType());

            var isFullTextSearchActivated = false;
            try
            {
                isFullTextSearchActivated = megaRoot.ConditionEvaluate("~PuC7Fh2WKv1H[Is Full Text Search Activated]");
            }
            catch
            {
                // ignored
            }
            if (isFullTextSearchActivated)
            {
                AddSearchAllField(query, genericObjectInterface);
            }

            var argumentsFactory = new GraphQLArgumentsFactory(_filterArgumentBuilder, _schemaMappingResolver);

            EnrichInterface(genericObjectInterface);

            // First generate base classes with basic properties
            foreach (var graphQlClass in _graphQlClasses)
            {
                GenerateClass(graphQlClass.Value, query, genericObjectInterface);
            }

            // Then generate relationships
            GenerateRelationships(argumentsFactory);

            //Then implement accessible classes from graphQL
            ImplementClasses(genericObjectInterface);

            return query;
        }

        private void ImplementClasses(GenericObjectInterface genericInterface)
        {
            //On ne prend pas toutes les classes pour éviter les classes "dans le vide" qui ne sont pas traitées par graphQL et non traitable pour le polymorphisme du resolvedType
            var classesToImplement = new HashSet<GraphQLClassDescription>();

            //D'abord on prend les classes racines
            foreach(var graphQlClassPair in _graphQlClasses)
            {
                var graphQlClass = graphQlClassPair.Value;
                if(graphQlClass.MetaClass.IsEntryPoint)
                {
                    classesToImplement.Add(graphQlClass);
                }
            }

            //Puis les relationships et on parcourt jusqu'à ce qu'on ait fait le tour des classes accessible depuis le schema graphQL
            var next = new Queue<GraphQLClassDescription>(classesToImplement);
            while(next.Count > 0)
            {
                var current = next.Dequeue();
                foreach(var relationship in current.Relationships)
                {
                    if(classesToImplement.Add(relationship.TargetClass)) //Pour s'assurer de ne pas traiter à l'infini
                    {
                        next.Enqueue(relationship.TargetClass);
                    }
                }
            }
            
            foreach(var graphQlClass in classesToImplement)
            {
                var classId = graphQlClass.MetaClass.Id;
                var type = _types[classId];
                genericInterface.ImplementInConcreteType(classId, type.GraphType);
            }
        }

        private void GenerateClass(GraphQLClassDescription graphQlClass, ComplexGraphType<IHopexDataModel> query, GenericObjectInterface genericObjectInterface)
        {
            var entity = graphQlClass.MetaClass;
            var type = new GenericObjectGraphType()
            {
                Description = entity.Description,
                Name = entity.Name
            };
            var typeInfo = new TypeInfo { GraphType = type, GraphQlDescription = graphQlClass, AggregationQueryType = CreateAggregationQuery(graphQlClass) };

            CreateSpecificFields(entity, type);
            CreateProperties(graphQlClass, type);
            //genericObjectInterface.ImplementInConcreteType(entity.Id, type);

            if (entity.IsEntryPoint)
            {
                var filterArguments = _filterArgumentBuilder.BuildFilterArguments(graphQlClass);
                var arguments = new QueryArguments(filterArguments)
                    {
                        new QueryArgument<IntGraphType> {Name = "first"},
                        new QueryArgument<StringGraphType> {Name = "after"},
                        new QueryArgument<IntGraphType> {Name = "last"},
                        new QueryArgument<StringGraphType> {Name = "before"},
                        new QueryArgument<IntGraphType> {Name = "skip"}
                    };

                query.Field<ListGraphType<ObjectGraphType<IModelElement>>>(
                        entity.Name,
                        entity.Description,
                        arguments,
                        (ctx) => GetElements(((UserContext)ctx.UserContext["usercontext"]).IRoot, ctx.Arguments, ctx.Source, ctx.Errors, graphQlClass)
                    )
                    .ResolvedType = new ListGraphType(type);

                var aggregationQuery = typeInfo.AggregationQueryType;
                query.Field<ListGraphType<ObjectGraphType<AggregationQueryType>>>(
                        aggregationQuery.Name,
                        aggregationQuery.Description,
                        arguments,
                        (ctx) => GetAggregatedElements(((UserContext)ctx.UserContext["usercontext"]).IRoot, ctx.SubFields, ctx.Arguments, ctx.Source, ctx.Errors, graphQlClass)
                    )
                    .ResolvedType = new ListGraphType(aggregationQuery);
            }
            _types.Add(entity.Id, typeInfo);
        }

        private void AddSearchAllField(ObjectGraphType<IHopexDataModel> query, GenericObjectInterface genericObjectInterface)
        {
            var searchAllFilter = new InputObjectGraphType<object> { Name = "searchAllFilter" };
            searchAllFilter.AddField(new FieldType { Name = "text", Type = typeof(StringGraphType) });
            searchAllFilter.AddField(new FieldType { Name = "minRange", Type = typeof(IntGraphType) });
            searchAllFilter.AddField(new FieldType { Name = "maxRange", Type = typeof(IntGraphType) });
            var searchAllOrderBy = new HopexEnumerationGraphType { Name = "searchAllOrderBy" };
            searchAllOrderBy.AddValue("ranking_ASC", "Order by ranking ascending", new Tuple<string, string>("Ranking", "ASC"));
            searchAllOrderBy.AddValue("ranking_DESC", "Order by ranking descending", new Tuple<string, string>("Ranking", "DESC"));
            searchAllOrderBy.AddValue("name_ASC", "Order by object name ascending", new Tuple<string, string>("ObjectName", "ASC"));
            searchAllOrderBy.AddValue("name_DESC", "Order by object name descending", new Tuple<string, string>("ObjectName", "DESC"));
            searchAllOrderBy.AddValue("objectTypeName_ASC", "Order by metaclass name ascending", new Tuple<string, string>("MetaclassName", "ASC"));
            searchAllOrderBy.AddValue("objectTypeName_DESC", "Order by metaclass name descending", new Tuple<string, string>("MetaclassName", "DESC"));
            var searchAllArguments = new QueryArguments
            {
                new QueryArgument(searchAllFilter) {Name = "filter"},
                new QueryArgument(new ListGraphType(searchAllOrderBy)) {Name = "orderBy"},
                new QueryArgument(LanguagesType) { Name = "language" }
            };
            query.Field<ListGraphType<ObjectGraphType<IModelElement>>>(
                    "SearchAll",
                    "Search any text in name and comment",
                    searchAllArguments,
                    (ctx) => ctx.Source.SearchAllAsync(ctx, _genericClass))
                .ResolvedType = new ListGraphType(genericObjectInterface);
        }



        private void CreateProperties(GraphQLClassDescription graphQlClass, ObjectGraphType<IModelElement> type)
        {
            CreateProperties<IModelElement>(graphQlClass.Properties, type, (ctx, prop) =>
            {
                string format = null;
                if (ctx.Arguments != null && ctx.Arguments.ContainsKey("format") && ctx.Arguments["format"].Value != null)
                {
                    format = ctx.Arguments["format"].Value?.ToString();
                }

                object result = null;
                var directives = ctx.FieldAst.Directives;
                var shouldBeSkipped = false;
                if (directives != null)
                {
                    foreach (var directive in directives)
                    {
                        switch (directive.Name)
                        {
                            case "isReadOnly":
                                if(!ctx.Source.IsReadOnly(prop))
                                {
                                    shouldBeSkipped = true;
                                }
                                break;
                            case "isReadWrite":
                                if (!ctx.Source.IsReadWrite(prop))
                                {
                                    shouldBeSkipped = true;
                                }
                                break;
                        }
                    }
                }
                if (shouldBeSkipped)
                {
                    return null;
                }

                IModelElement targetElement = ctx.Source;
                result = targetElement.GetValue<object>(prop, ctx.Arguments, format);
                //Logger.LogInformation($"  SchemaBuilder.GetValue of {ctx.Source.MegaObject?.MegaField}.{prop.Name} (including get CRUD) ended in: {stopwatch.ElapsedMilliseconds} ms");
                if (result == null)
                {
                    return null;
                }

                if (prop.PropertyType == PropertyType.Date)
                {
                    if (ctx.Arguments != null && ctx.Arguments.TryGetValue("timeOffset", out var timeOffsetValue) && timeOffsetValue.Value is string timeOffset)
                    {
                        try
                        {
                            var date = DateTime.Parse(result.ToString());
                            var timeSpan = TimeSpan.Parse(timeOffset.TrimStart('+'));
                            date = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, TimeZoneInfo.CreateCustomTimeZone("MyTimeZone", timeSpan, "MyTimeZone", "MyTimeZone"));
                            result = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
                        }
                        catch (Exception e)
                        {
                            throw new ExecutionError($"Time Offset must be a valid value between -14:00 and +14:00 (value: {timeOffset} is not accepted).", e);
                        }
                    }
                    if (DateTime.TryParse(result.ToString(), out var dateToFormat))
                    {
                        result = dateToFormat.ToString(!string.IsNullOrEmpty(format) ? format : "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                }

                if ((directives?.Count ?? 0) > 0)
                {
                    result = ApplyCustomDirectives(ctx.FieldAst.Directives, result, prop.PropertyType);
                }

                return result;
            });
        }

        private static string TrimAndSetSeparatorBetweenWords(string input, string separator)
        {
            var regexTrim = new Regex(@"^[\W_]+|[\W_]+$");
            var result = regexTrim.Replace(input, "");
            var regexNonAlphaNumeric = new Regex(@"[\W_]+");
            return regexNonAlphaNumeric.Replace(result, separator);
        }

        private static object ApplyCustomDirectives(Directives directives, object value, PropertyType propertyType)
        {
            if (directives.Count <= 0 || value == null)
            {
                return value;
            }
            foreach (var directive in directives)
            {
                switch (directive.Name)
                {
                    case "capitalize" when value is string val:
                        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val);
                    case "deburr" when value is string val:
                        return RemoveDiacritics(val);
                    case "pascalCase" when value is string val:
                        val = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val);
                        return TrimAndSetSeparatorBetweenWords(val, "");
                    case "camelCase" when value is string val:
                        val = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val);
                        val = TrimAndSetSeparatorBetweenWords(val, "");
                        if (!string.IsNullOrEmpty(val) && char.IsUpper(val[0]))
                        {
                            val = char.ToLower(val[0]) + val.Substring(1);
                        }
                        return val;
                    case "kebabCase" when value is string val:
                        return TrimAndSetSeparatorBetweenWords(val.ToLower(), "-");
                    case "snakeCase" when value is string val:
                        return TrimAndSetSeparatorBetweenWords(val.ToLower(), "_");
                    case "toLower" when value is string val:
                        return val.ToLower();
                    case "lowerFirst" when value is string val:
                        if (!string.IsNullOrEmpty(val) && char.IsUpper(val[0]))
                        {
                            val = char.ToLower(val[0]) + val.Substring(1);
                        }
                        return val;
                    case "toUpper" when value is string val:
                        return val.ToUpper();
                    case "upperFirst" when value is string val:
                        if (!string.IsNullOrEmpty(val) && char.IsLower(val[0]))
                        {
                            val = char.ToUpper(val[0]) + val.Substring(1);
                        }
                        return val;
                    case "trim" when value is string val:
                        return val.Trim();
                    case "trimStart" when value is string val:
                        return val.TrimStart();
                    case "trimEnd" when value is string val:
                        return val.TrimEnd();
                    case "phone" when value is string val:
                        {
                            val = val.Replace("+", "").Replace("-", "")
                                .Replace("(", "").Replace(")", "")
                                .Replace("[", "").Replace("]", "")
                                .Replace(" ", "");
                            if (long.TryParse(val, out var phoneNumber))
                            {
                                var format = directive.Arguments?.ValueFor("format")?.Value?.ToString();
                                return string.Format(format ?? string.Empty, phoneNumber);
                            }
                            return $"Unable to format {value} as phone number";
                        }
                    case "number" when value is int || value is long || value is double || value is float || value is decimal:
                        {
                            var format = directive.Arguments?.ValueFor("format")?.Value?.ToString();
                            format = format ?? string.Empty;

                            switch (value)
                            {
                                case int i:
                                    return i.ToString(format, CultureInfo.InvariantCulture);
                                case long l:
                                    return l.ToString(format, CultureInfo.InvariantCulture);
                                case double d:
                                    return d.ToString(format, CultureInfo.InvariantCulture);
                                case float f:
                                    return f.ToString(format, CultureInfo.InvariantCulture);
                                case decimal m:
                                    return m.ToString(format, CultureInfo.InvariantCulture);
                            }
                            break;
                        }
                    case "currency" when value is int || value is long || value is double || value is float || value is decimal:
                        {
                            var format = directive.Arguments?.ValueFor("format")?.Value?.ToString();
                            format = format ?? string.Empty;

                            var currency = directive.Arguments?.ValueFor("currency")?.Value?.ToString();
                            switch (value)
                            {
                                case int i:
                                    return string.IsNullOrEmpty(currency) ? i.ToString(format, CultureInfo.InvariantCulture) : i.ToString(format, CultureInfo.InvariantCulture) + " " + currency;
                                case long l:
                                    return string.IsNullOrEmpty(currency) ? l.ToString(format, CultureInfo.InvariantCulture) : l.ToString(format, CultureInfo.InvariantCulture) + " " + currency;
                                case double d:
                                    return string.IsNullOrEmpty(currency) ? d.ToString(format, CultureInfo.InvariantCulture) : d.ToString(format, CultureInfo.InvariantCulture) + " " + currency;
                                case float f:
                                    return string.IsNullOrEmpty(currency) ? f.ToString(format, CultureInfo.InvariantCulture) : f.ToString(format, CultureInfo.InvariantCulture) + " " + currency;
                                case decimal m:
                                    return string.IsNullOrEmpty(currency) ? m.ToString(format, CultureInfo.InvariantCulture) : m.ToString(format, CultureInfo.InvariantCulture) + " " + currency;
                            }
                            break;
                        }
                    case "date" when value is DateTime || value is DateTimeOffset || value is TimeSpan || propertyType == PropertyType.Date:
                        {
                            var format = directive.Arguments?.ValueFor("format")?.Value?.ToString();
                            format = format == null ? string.Empty : format.Replace(":", "\\:");

                            switch (value)
                            {
                                case DateTime dateTime:
                                    return dateTime.ToString(format, CultureInfo.InvariantCulture);
                                case DateTimeOffset dateTimeOffset:
                                    return dateTimeOffset.ToString(format, CultureInfo.InvariantCulture);
                                case TimeSpan timeSpan:
                                    return timeSpan.ToString(format, CultureInfo.InvariantCulture);
                                default:
                                    var date = DateTime.Parse(value.ToString());
                                    return date.ToString(format, CultureInfo.InvariantCulture);
                            }
                        }
                }
            }
            return value;
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
                    resolve: ctx => $"{((UserContext)ctx.UserContext["usercontext"]).WebServiceUrl}/api/attachment/{ctx.Source.Id}/file");

            var hasDownloadUrl = hasUploadUrl || entity.Id == ID_BUSINESS_DOCUMENT_VERSION || entity.Id == ID_SYSTEM_BUSINESS_DOCUMENT_VERSION;
            if (hasDownloadUrl)
                type.Field(
                    typeof(StringGraphType),
                    "downloadUrl",
                    "Download document from here",
                    resolve: ctx => $"{((UserContext)ctx.UserContext["usercontext"]).WebServiceUrl}/api/attachment/{ctx.Source.Id}/file");

            var hasDiagramImageUrl = entity.Id == ID_DIAGRAM || entity.Id == ID_SYSTEM_DIAGRAM;
            if (hasDiagramImageUrl)
                type.Field(
                    typeof(StringGraphType),
                    "downloadUrl",
                    "Download diagram image from here",
                    resolve: ctx => $"{((UserContext)ctx.UserContext["usercontext"]).WebServiceUrl}/api/diagram/{ctx.Source.Id}/image");

            _schemaMappingResolver.CreateField(entity, type);
        }

        static private readonly Dictionary<string, ArgumentValue> NO_ARGUMENTS = new Dictionary<string, ArgumentValue>();

        private void GenerateRelationships(GraphQLArgumentsFactory argumentsFactory)
        {
            foreach (var typeInfo in _types)
            {
                var graphQlClass = typeInfo.Value.GraphQlDescription;
                var type = typeInfo.Value.GraphType;
                foreach (var graphQlRelationship in graphQlClass.Relationships)
                {
                    var relationship = graphQlRelationship.MetaAssociation;
                    var targetType = _types[relationship.TargetClass.Id];
                    var targetGraphType = targetType.GraphType;
                    var aggregationQueryType = targetType.AggregationQueryType;

                    //On créé la relationship
                    CreateRelationship(argumentsFactory, graphQlRelationship, type, targetGraphType, aggregationQueryType);
                    _schemaMappingResolver.CreateField(relationship, targetGraphType);
                }
            }
        }

        private void CreateRelationship(GraphQLArgumentsFactory argumentsFactory, GraphQLRelationshipDescription graphQlRelationship, ObjectGraphType<IModelElement> type, IGraphType targetClassType, AggregationQueryType aggregationQuery)
        {
            var relationship = graphQlRelationship.MetaAssociation;
            var arguments = argumentsFactory.BuildRelationshipArguments(graphQlRelationship, LanguagesType);

            type.FieldAsync<ListGraphType<ObjectGraphType<IModelElement>>>(
                    relationship.Name,
                    relationship.Description,
                    arguments,
                    async ctx => await GetElements(((UserContext)ctx.UserContext["usercontext"]).IRoot, ctx.Arguments ?? NO_ARGUMENTS, ctx.Source, ctx.Errors, graphQlRelationship.TargetClass, relationship.Name)
                )
                .ResolvedType = new ListGraphType(targetClassType);

            type.FieldAsync<ListGraphType<ObjectGraphType<AggregationQueryType>>>(
                    relationship.Name + "AggregatedValues",
                    relationship.Description,
                    arguments,
                    async ctx => await GetAggregatedElements(((UserContext)ctx.UserContext["usercontext"]).IRoot, ctx.SubFields, ctx.Arguments ?? NO_ARGUMENTS, ctx.Source, ctx.Errors, graphQlRelationship.TargetClass, relationship.Name)
                )
                .ResolvedType = new ListGraphType(aggregationQuery);
        }

        private static AggregationQueryType CreateAggregationQuery(GraphQLClassDescription graphQlClass)
        {
            var entity = graphQlClass.MetaClass;
            var aggregationQuery = new AggregationQueryType
            {
                Name = ToValidName(entity.Name) + "AggregatedValues",
                Description = entity.Description
            };

            foreach (var graphQlProperty in graphQlClass.Properties)
            {
                var property = graphQlProperty.MetaAttribute;
                var propertyType = typeof(CustomFloatGraphType);
                QueryArguments arguments;
                if (property.PropertyType == PropertyType.Int || property.PropertyType == PropertyType.Long || property.PropertyType == PropertyType.Double || property.PropertyType == PropertyType.Currency)
                {
                    arguments = new QueryArguments(new QueryArgument(typeof(AggregateNumbersFunctionType)) { Name = "function" });
                }
                else
                {
                    arguments = new QueryArguments(new QueryArgument(typeof(AggregateFunctionType)) { Name = "function" });
                }
                var field = new FieldType
                {
                    Type = propertyType,
                    Name = graphQlProperty.Name,
                    Description = property.Description,
                    Arguments = arguments,
                    Resolver = new FuncFieldResolver<double>(ctx =>
                    {
                        var aggregationQueryResult = ((AggregationQueryType)ctx.Source).AggregationQueryResultType;
                        var fieldName = !string.IsNullOrEmpty(ctx.FieldAst.Alias) ? $"{ctx.FieldAst.Alias}:{ctx.FieldAst.Name}" : ctx.FieldAst.Name;
                        return aggregationQueryResult.AggregatedValues[fieldName];
                    })
                };
                aggregationQuery.AddField(field);
            }

            return aggregationQuery;
        }

        private IObjectGraphType CreateMutationSchema(IHopexMetaModel hopexSchema)
        {
            var mutation = new ObjectGraphType<IHopexDataModel>
            {
                Name = "Mutation"
            };

            var mutationListFactory = new MutationListTypeFactory(this);

            mutation.Field<CurrentContextForMutationType>(
                "_updateCurrentContext",
                "Update current context values",
                new QueryArguments(new QueryArgument<NonNullGraphType<CurrentContextForMutationInputType>>
                {
                    Name = "currentContext"
                }),
                context =>
                {
                    var currentContext = context.GetArgument<Dictionary<string, object>>("currentContext");
                    if (!(currentContext?["language"] is IMegaObject language))
                    {
                        return null;
                    }
                    var userContext = (UserContext)context.UserContext["usercontext"];
                    var root = userContext.MegaRoot;
                    root.CurrentEnvironment.NativeObject.SetCurrentLanguage(language.MegaUnnamedField.Substring(0, 13));
                    var personSystem = root.GetObjectFromId<MegaObject>(root.CurrentEnvironment.CurrentUserId);
                    personSystem.NativeObject.SetProp(MetaAttributeLibrary.DataLanguage, language.MegaUnnamedField.Substring(0, 13));
                    var resultLanguageId = root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentLanguageId);
                    var resultLanguageCode = userContext.Languages.FirstOrDefault(x => x.Value.MegaUnnamedField.Substring(1, 12) == resultLanguageId).Key;
                    return new CurrentContextForMutationResultType
                    {
                        Language = resultLanguageCode
                    };
                });

            
            // Two phases
            // First generate input
            foreach (var graphQlClass in _graphQlClasses)
            {
                var entity = graphQlClass.Value.MetaClass;
                var typeInput = new InputObjectGraphType<object>
                {
                    Description = $"Input type for {entity.Name}",
                    Name = "Input" + entity.Name
                };

                var typeUniqueInput = new InputObjectGraphType<object>
                {
                    Description = $"Input type for {entity.Name} including unique fields",
                    Name = "InputUnique" + entity.Name
                };

                var types = new List<InputObjectGraphType<object>> { typeInput, typeUniqueInput };
                // Add arguments
                foreach (var type in types)
                {
                    var graphQlProperties = graphQlClass.Value.Properties;
                    var filteredProperties = graphQlProperties.Where(p => !p.MetaAttribute.IsReadOnly && (type != typeInput || !p.MetaAttribute.IsUnique));
                    CreateProperties<IModelElement>(filteredProperties, type, (ctx, prop) =>
                    {
                        string format = null;
                        if (ctx.Arguments != null && ctx.Arguments.ContainsKey("format") && ctx.Arguments["format"].Value != null)
                        {
                            format = ctx.Arguments["format"].ToString();
                        }
                        var val = ctx.GetArgument<object>(prop.Name, format);
                        var elem = ctx.Source;
                        elem.SetValue(prop, val, format);
                        return val;
                    }, true);

                    // Then generate relationships
                    foreach (var graphQlRelationship in graphQlClass.Value.Relationships.Where(r => !r.MetaAssociation.IsReadOnly))
                    {
                        var listType = mutationListFactory.CreateMutationList(graphQlRelationship);
                        var field = new FieldType
                        {
                            ResolvedType = listType,
                            Name = graphQlRelationship.MetaAssociation.Name,
                            Description = graphQlRelationship.MetaAssociation.Description,
                        };
                        type.AddField(field);
                    }

                    InputCustomPropertyType.AddCustomFields(type);
                    InputCustomRelationshipType.AddCustomRelations(type);
                }

                if(entity.IsEntryPoint)
                {
                    var enumCreationMode = new HopexEnumerationGraphType { Name = "creationMode" };
                    enumCreationMode.AddValue("RAW", "Do not use InstanceCreator, but classic \"create mode\"", false);
                    enumCreationMode.AddValue("BUSINESS", "Use InstanceCreator", true);
                    var queryArgumentsCreate = new QueryArguments(
                        new QueryArgument(typeof(StringGraphType)) { Name = "id" },
                        new QueryArgument(typeof(IdType)) { Name = "idType" },
                        new QueryArgument(new NonNullGraphType(typeUniqueInput)) { Name = entity.Name.ToCamelCase() });
                    if(!Equals(entity.Id, "~UkPT)TNyFDK5"))
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
                            if(ctx.HasArgument("creationMode"))
                            {
                                useInstanceCreator = ctx.GetArgument<bool>("creationMode");
                            }
                            string id = null;
                            if(ctx.HasArgument("id"))
                            {
                                id = ctx.GetArgument<string>("id");
                            }
                            var idType = IdTypeEnum.INTERNAL;
                            if(ctx.HasArgument("idType"))
                            {
                                idType = ctx.GetArgument<IdTypeEnum>("idType");
                            }
                            var result = await ctx.Source.CreateElementAsync(
                                entity,
                                id,
                                idType,
                                useInstanceCreator,
                                CreateSetters(graphQlClass.Value, ctx));
                            AddRangeExecutionErrors(ctx.FieldAst, ctx.Errors, result.Errors);
                            return result;
                        }).ResolvedType = new NonNullGraphType(_types [entity.Id].GraphType);
                    
                    mutation.FieldAsync<ObjectGraphType<object>>(
                        "CreateUpdate" + entity.Name,
                        $"Create or update a {entity.Name}",
                        queryArgumentsCreate,
                        async (ctx) =>
                        {
                            var useInstanceCreator = false;
                            if(ctx.Arguments.ContainsKey("creationMode") && ctx.Arguments ["creationMode"].Value != null)
                            {
                                useInstanceCreator = (bool)ctx.Arguments ["creationMode"].Value;
                            }
                            string id = null;
                            if(ctx.Arguments != null && ctx.Arguments.ContainsKey("id") && ctx.Arguments ["id"].Value != null)
                            {
                                id = ctx.Arguments ["id"].Value.ToString();
                            }
                            var idType = IdTypeEnum.INTERNAL;
                            if(ctx.Arguments.ContainsKey("idType") && ctx.Arguments ["idType"].Value != null)
                            {
                                Enum.TryParse(ctx.Arguments ["idType"].Value.ToString(), out idType);
                            }
                            var result = await ctx.Source.CreateUpdateElementAsync(
                                entity,
                                id,
                                idType,
                                CreateSetters(graphQlClass.Value, ctx),
                                useInstanceCreator);
                            AddRangeExecutionErrors(ctx.FieldAst, ctx.Errors, result.Errors);
                            return result;
                        })
                    .ResolvedType = new NonNullGraphType(_types [entity.Id].GraphType);
                    
                    var updateArguments = new QueryArguments
                    {
                        new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "id"},
                        new QueryArgument(typeof(IdType)) {Name = "idType"},
                        new QueryArgument(new NonNullGraphType(typeUniqueInput)) { Name = entity.Name.ToCamelCase() }
                    };
                    mutation.FieldAsync<ObjectGraphType<object>>(
                            "Update" + entity.Name,
                            $"Update a {entity.Name}",
                            updateArguments,
                            async ctx => await UpdateElement(ctx, graphQlClass.Value))
                        .ResolvedType = new NonNullGraphType(_types [entity.Id].GraphType);


                    var updateManyArguments = _filterArgumentBuilder.BuildFilterArguments(graphQlClass.Value);
                    updateManyArguments.Add(
                        new QueryArgument(new NonNullGraphType(typeInput)) { Name = entity.Name.ToCamelCase() }
                    );

                    mutation.FieldAsync<ObjectGraphType<object>>(
                            "UpdateMany" + entity.Name,
                            $"Update multiple {entity.Name}",
                            updateManyArguments,
                            async ctx => await UpdateManyElements(((UserContext)ctx.UserContext ["usercontext"]).IRoot, ctx, graphQlClass.Value))
                        .ResolvedType = new ListGraphType(_types [entity.Id].GraphType);

                    var deleteArguments = new QueryArguments
                    {
                        new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "id"},
                        new QueryArgument(typeof(IdType)) {Name = "idType"},
                        new QueryArgument<BooleanGraphType> {Name = "cascade"}
                    };
                    mutation.FieldAsync<DeleteType>(
                        "Delete" + entity.Name,
                        $"Delete a {entity.Name}",
                        deleteArguments,
                        async ctx => await DeleteElement(((UserContext)ctx.UserContext ["usercontext"]).IRoot, ctx, entity));

                    var deleteManyArguments = _filterArgumentBuilder.BuildFilterArguments(graphQlClass.Value);
                    deleteManyArguments.Add(
                        new QueryArgument<BooleanGraphType> { Name = "cascade" }
                    );
                    mutation.FieldAsync<DeleteType>(
                            "DeleteMany" + entity.Name,
                            $"Delete multiple {entity.Name}",
                            deleteManyArguments,
                            async ctx => await DeleteManyElements(((UserContext)ctx.UserContext ["usercontext"]).IRoot, ctx, graphQlClass.Value));
                }
            }

            return mutation;
        }

        internal void CreateProperties<T>(IEnumerable<GraphQLPropertyDescription> graphQlProperties, IComplexGraphType type, Func<IResolveFieldContext<T>, IPropertyDescription, object> resolver, bool isMutation = false)
        {
            foreach (var graphQlProperty in graphQlProperties)
            {
                var prop = graphQlProperty.MetaAttribute;
                var propertyName = graphQlProperty.Name;
                var propertyType = TypeExtensions.GetGraphTypeFromType(prop.NativeType);
                IGraphType resolvedType = null;

                if (prop.EnumValues?.Count() > 0)
                {
                    propertyType = null;
                    resolvedType = GetOrCreateEnumType(prop);
                }

                if (prop.PropertyType == PropertyType.Id && prop.Id != MetaAttributeLibrary.AbsoluteIdentifier.Substring(0, 13) && !isMutation)
                {
                    var field = new FieldType
                    {
                        Type = typeof(GenericObjectInterface),
                        ResolvedType = null,
                        Name = propertyName,//.TrimEnd("Id".ToCharArray()),
                        Description = prop.Description,
                        Resolver = new FuncFieldResolver<IModelElement, object>(ctx =>
                        {
                            var id = ctx.Source.GetValue<string>(prop);
                            return id == null ? null : ctx.Source.DomainModel.GetElementByIdAsync(_genericClass, id, IdTypeEnum.INTERNAL).Result;
                        })
                    };
                    type.AddField(field);
                }
                else
                {
                    var field = new FieldType
                    {
                        Type = propertyType,
                        ResolvedType = resolvedType,
                        Name = propertyName,
                        Description = prop.Description,
                        Resolver = new FuncFieldResolver<T, object>(ctx => resolver(ctx, prop)),
                        Arguments = new QueryArguments()
                    };
                    if (!isMutation)
                    {
                        if (prop.IsFormattedText)
                        {
                            field.Arguments.Add(new QueryArgument(typeof(StringFormat)) { Name = "format" });
                        }
                        if (prop.IsTranslatable)
                        {
                            field.Arguments.Add(new QueryArgument(LanguagesType) { Name = "language" });
                        }
                        if (prop.Id == MetaAttributeLibrary.ShortName.Remove(13))
                        {
                            field.Arguments.Add(new QueryArgument(typeof(NameSpaceFormat)) { Name = "nameSpace" });
                        }
                        switch (prop.PropertyType)
                        {
                            case PropertyType.Date:
                                field.Arguments.Add(new QueryArgument(typeof(StringGraphType)) { Name = "format" });
                                field.Arguments.Add(new QueryArgument(typeof(StringGraphType))
                                {
                                    Name = "timeOffset",
                                    Description = "You can specify your time zone in format +hh:mm or -hh:mm"
                                });
                                break;
                            case PropertyType.Currency:
                                field.Arguments.Add(new QueryArgument(CurrenciesType) { Name = "currency" });
                                field.Arguments.Add(new QueryArgument(typeof(CustomDateGraphType)) { Name = "dateRate" });
                                break;
                            case PropertyType.Enum:
                                field.Arguments.Add(new QueryArgument(typeof(EnumFormat)) { Name = "format" });
                                break;
                        }
                    }

                    type.AddField(field);

                    if (prop.Id == MetaAttributeLibrary.DataLanguage.Substring(0, 13))
                    {
                        var languageField = new FieldType
                        {
                            Type = typeof(LanguagesEnumerationGraphType),
                            ResolvedType = LanguagesType,
                            Name = "dataLanguageCode",
                            Description = prop.Description,
                            Resolver = new FuncFieldResolver<T, object>(context =>
                            {
                                var languageId = ((IModelElement)context.Source).GetValue<object>(prop).ToString();
                                var userContext = (UserContext)context.UserContext["usercontext"];
                                var languageCode = userContext.Languages.FirstOrDefault(x => x.Value.MegaUnnamedField.Substring(1, 12) == languageId).Key;
                                return languageCode;
                            }),
                            Arguments = new QueryArguments()
                        };
                        type.AddField(languageField);
                    }
                }
            }
        }

        private IEnumerable<ISetter> CreateSetters(GraphQLClassDescription graphQlClass, IResolveFieldContext<IHopexDataModel> ctx = null)
        {
            var arguments = (IDictionary<string, object>)ctx.Arguments[graphQlClass.MetaClass.Name.ToCamelCase()].Value;
            return graphQlClass.CreateSetter(arguments, _graphQlClasses);
        }

        private async Task<IEnumerable<IModelElement>> EnumerateRootElements(IMegaRoot root, IDictionary<string, ArgumentValue> arguments, IHasCollection source, GraphQLClassDescription graphQlClass, string relationshipName)
        {
            string erql = null;
            var list = new List<IModelElement>();
            var sourceElement = source as IModelElement;

            IMegaObject language = null;
            if (arguments.TryGetValue("language", out var languageValue) && languageValue.Value is IMegaObject value)
            {
                language = value;
            }

            var entity = graphQlClass.MetaClass;
            if (arguments.TryGetValue("filter", out var obj) && obj.Value is Dictionary<string, object> filter)
            {
                GraphQLRelationshipDescription graphQlRelationship = null;
                if(sourceElement != null)
                {
                    var type = _types.First(t => t.Value.Description == sourceElement.ClassDescription).Value;
                    var sourceGraphQlClass = type.GraphQlDescription;
                    graphQlRelationship = sourceGraphQlClass.Relationships.FirstOrDefault(r => r.Name.Equals(relationshipName, StringComparison.OrdinalIgnoreCase));
                }
                var compiler = new FilterCompiler(root, graphQlClass, graphQlRelationship, sourceElement);
                erql = $"SELECT {entity.Id}[{entity.Name}]";
                erql += " WHERE " + compiler.CreateHopexQuery(filter, language);
                Logger.LogInformation($"{erql}");
                if (root is ISupportsDiagnostics diags)
                {
                    diags.AddGeneratedERQL(erql);
                }
            }

            var orderByClauses = new List<Tuple<string, int>>();
            if (arguments.TryGetValue("orderBy", out var orderByArgument) && orderByArgument.Value != null)
            {
                if (!(orderByArgument.Value is IEnumerable<object>))
                {
                    throw new ExecutionError("Order by must be an array of 1 to 3 values maximum. Ex: query{application(orderBy:[name_ASC]){id,name}}");
                }
                var orderBy = (IEnumerable<object>)orderByArgument.Value;
                orderByClauses.AddRange(orderBy.Cast<Tuple<string, int>>());
            }

            var getCollectionArguments = new GetCollectionArguments()
            {
                Erql = erql,
                OrderByClauses = orderByClauses,
                AdHocPredicate = _schemaMappingResolver.CreateSchemaPredicate(arguments) ?? (e => true)
            };

            var collection = await source.GetCollectionAsync(entity.Name, relationshipName, getCollectionArguments);
            //var orderedCollection = OrderCollection(collection, orderByClauses);
            var paginatedModelElements = GetPaginatedModelElements(collection.ToList(), arguments).ToList();
            return language != null ? ApplyLanguageToResults(paginatedModelElements, language) : paginatedModelElements;
        }

        private static IEnumerable<IModelElement> ApplyLanguageToResults(IEnumerable<IModelElement> modelElements, IMegaObject language)
        {
            var results = modelElements.ToList();
            foreach (var modelElement in results)
            {
                modelElement.Language = language;
            }
            return results;
        }

        public static IEnumerable<IModelElement> GetPaginatedModelElements(IReadOnlyList<IModelElement> orderedCollection, IDictionary<string, ArgumentValue> arguments)
        {
            var list = new List<IModelElement>();
            int? first = null;
            var after = "";
            int? last = null;
            var before = "";
            var skip = 0;

            if (arguments.TryGetValue("first", out var obj) && obj.Value != null)
            {
                first = (int)obj.Value;
            }
            if (arguments.TryGetValue("after", out obj) && obj.Value != null)
            {
                after = (string)obj.Value;
            }
            if (arguments.TryGetValue("last", out obj) && obj.Value != null)
            {
                last = (int)obj.Value;
            }
            if (arguments.TryGetValue("before", out obj) && obj.Value != null)
            {
                before = (string)obj.Value;
            }
            if (arguments.TryGetValue("skip", out obj) && obj.Value != null)
            {
                skip = (int)obj.Value;
            }

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

        private static List<IModelElement> OrderCollection(IModelCollection collection, List<Tuple<string, int>> orderByClauses)
        {
            var orderedCollection = collection.ToList();
            if (orderByClauses.Count > 0)
            {
                IOrderedEnumerable<IModelElement> tmp;
                if (orderByClauses[0].Item2 == 1)
                {
                    tmp = collection.OrderBy(x => x.MegaObject.GetPropertyValue(orderByClauses[0].Item1));
                }
                else //if (orderByClauses[0].Item2 == -1)
                {
                    tmp = collection.OrderByDescending(x => x.MegaObject.GetPropertyValue(orderByClauses[0].Item1));
                }
                for (var i = 1; i < orderByClauses.Count; i++)
                {
                    if (orderByClauses[i].Item2 == 1)
                    {
                        var i1 = i;
                        tmp = tmp.ThenBy(x => x.MegaObject.GetPropertyValue(orderByClauses[i1].Item1));
                    }
                    else //if (orderByClauses[0].Item2 == -1)
                    {
                        var i1 = i;
                        tmp = tmp.ThenByDescending(x => x.MegaObject.GetPropertyValue(orderByClauses[i1].Item1));
                    }
                }
                orderedCollection = tmp.ToList();
            }
            return orderedCollection;
        }

        private static void AddRangeExecutionErrors(Field fieldAst, ExecutionErrors executionErrors, IEnumerable<Exception> errors)
        {
            foreach (var error in errors)
            {
                var prefix = $"Error occured on {fieldAst?.Alias ?? fieldAst?.Name ?? "null"}: ";
                executionErrors.Add(new ExecutionError(prefix + error.Message + " See more details in log file", error.InnerException));
            }
        }

        private async Task<IEnumerable<IModelElement>> GetElements(IMegaRoot iRoot, IDictionary<string, ArgumentValue> arguments, IHasCollection source, ExecutionErrors errors, GraphQLClassDescription graphQlClass, string linkName = null)
        {
            var entity = graphQlClass.MetaClass;
            var permissions = CrudComputer.GetCollectionMetaPermission(iRoot, entity.Id);
            if (!permissions.IsReadable)
                return Enumerable.Empty<IModelElement>();

            if (arguments.TryGetValue("filter", out var obj) && obj.Value is Dictionary<string, object> filter)
            {
                foreach (var filteredProperty in filter)
                {
                    var property = graphQlClass.Properties.FirstOrDefault(x => string.Equals(x.Name, filteredProperty.Key, StringComparison.OrdinalIgnoreCase))?.MetaAttribute;
                    if (property != null && filteredProperty.Value is string valueString && valueString.Length > property.MaxLength)
                    {
                        errors.Add(new ExecutionError($"Value {filteredProperty.Value} for {property.Name} exceeds maximum length of {property.MaxLength}\n"));
                    }
                }
                if (errors.Count > 0)
                {
                    return new List<IModelElement>();
                }
            }
            var elements = await EnumerateRootElements(iRoot, arguments, source, graphQlClass, linkName);
            return elements;
        }

        private async Task<IEnumerable<AggregationQueryType>> GetAggregatedElements(IMegaRoot iRoot, IDictionary<string, Field> subFields, IDictionary<string, ArgumentValue> arguments, IHasCollection source, ExecutionErrors errors, GraphQLClassDescription graphQlClass, string linkName = null)
        {
            var megaCollection = await GetElements(iRoot, arguments, source, errors, graphQlClass, linkName);
            var modelElements = megaCollection.ToList();

            var aggregatedElement = new AggregationQueryType { AggregationQueryResultType = new AggregationQueryResultType { Count = modelElements.Count() } };
            foreach (var field in subFields.Values)
            {
                var fieldName = !string.IsNullOrEmpty(field.Alias) ? $"{field.Alias}:{field.Name}" : field.Name;
                var property = graphQlClass.Properties.FirstOrDefault(x => string.Equals(x.Name, field.Name, StringComparison.CurrentCultureIgnoreCase))?.MetaAttribute;
                if (property == null)
                {
                    throw new ExecutionError($"Field {fieldName} is not queryable.");
                }
                var aggregateFunctionValue = field.Arguments.ValueFor("function");
                if (aggregateFunctionValue == null)
                {
                    throw new ExecutionError($"You must specify an aggregated function for the field: {fieldName}. Example: {fieldName}(aggregateFunction:SUM)");
                }
                var value = double.NaN;
                if (Enum.TryParse((string)aggregateFunctionValue.Value, true, out AggregateFunctionTypeEnum aggregateFunction))
                {
                    switch (aggregateFunction)
                    {
                        case AggregateFunctionTypeEnum.COUNT:
                            value = modelElements.Count(x => x.GetValue<object>(property) != null);
                            break;
                        case AggregateFunctionTypeEnum.COUNTBLANK:
                            value = modelElements.Count(x => x.GetValue<object>(property) == null);
                            break;
                        default:
                            throw new ExecutionError($"The function {aggregateFunction} is not available for the field: {fieldName}.");
                    }
                }
                else if (Enum.TryParse((string)aggregateFunctionValue.Value, true, out AggregateNumbersFunctionTypeEnum aggregateNumbersFunction))
                {
                    switch (aggregateNumbersFunction)
                    {
                        case AggregateNumbersFunctionTypeEnum.COUNT:
                            value = modelElements.Count(x => x.GetValue<object>(property) != null);
                            break;
                        case AggregateNumbersFunctionTypeEnum.COUNTBLANK:
                            value = modelElements.Count(x => x.GetValue<object>(property) == null);
                            break;
                        case AggregateNumbersFunctionTypeEnum.SUM:
                            value = modelElements.Sum(element => Convert.ToDouble(element.GetValue<object>(property)));
                            break;
                        case AggregateNumbersFunctionTypeEnum.AVERAGE:
                            value = modelElements.Average(element => Convert.ToDouble(element.GetValue<object>(property)));
                            break;
                        case AggregateNumbersFunctionTypeEnum.MEDIAN:
                            {
                                var data = modelElements.Select(element => Convert.ToDouble(element.GetValue<object>(property))).ToArray();
                                Array.Sort(data);
                                if (data.Length % 2 == 0)
                                {
                                    value = (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2;
                                }
                                else
                                {
                                    value = data[data.Length / 2];
                                }
                                break;
                            }
                        case AggregateNumbersFunctionTypeEnum.MIN:
                            value = modelElements.Min(element => Convert.ToDouble(element.GetValue<object>(property)));
                            break;
                        case AggregateNumbersFunctionTypeEnum.MAX:
                            value = modelElements.Max(element => Convert.ToDouble(element.GetValue<object>(property)));
                            break;
                        default:
                            throw new ExecutionError($"The function {aggregateNumbersFunction} is not available for the field: {fieldName}.");
                    }
                }
                aggregatedElement.AggregationQueryResultType.AggregatedValues.Add(fieldName, value);
            }

            return new List<AggregationQueryType> { aggregatedElement };
        }

        private async Task<IModelElement> UpdateElement(IResolveFieldContext<IHopexDataModel> ctx, GraphQLClassDescription graphQlClass)
        {
            string id = null;
            if (ctx.Arguments != null && ctx.Arguments.ContainsKey("id") && ctx.Arguments["id"].Value != null)
            {
                id = ctx.Arguments["id"].Value.ToString();
            }

            var idType = IdTypeEnum.INTERNAL;
            if (ctx.Arguments != null && ctx.Arguments.ContainsKey("idType") && ctx.Arguments["idType"].Value != null)
            {
                Enum.TryParse(ctx.Arguments["idType"].Value.ToString(), out idType);
            }

            var updatedElement = await ctx.Source.UpdateElementAsync(graphQlClass.MetaClass, id, idType, CreateSetters(graphQlClass, ctx));
            AddRangeExecutionErrors(ctx.FieldAst, ctx.Errors, updatedElement.Errors);
            return updatedElement;
        }

        private async Task<IEnumerable<IModelElement>> UpdateManyElements(IMegaRoot root, IResolveFieldContext<IHopexDataModel> ctx, GraphQLClassDescription graphQlClass)
        {
            var updatedElements = new List<IModelElement>();
            if (ctx.Arguments.TryGetValue("filter", out var obj) && obj.Value is Dictionary<string, object>)
            {
                var elementsToUpdate = await GetElements(root, ctx.Arguments, ctx.Source, ctx.Errors, graphQlClass);
                foreach (var element in elementsToUpdate)
                {
                    await element.UpdateAsync(CreateSetters(graphQlClass, ctx));
                    AddRangeExecutionErrors(ctx.FieldAst, ctx.Errors, element.Errors);
                    updatedElements.Add(element);
                }
            }
            return updatedElements;
        }


        private async Task<DeleteResultType> DeleteElement(IMegaRoot root, IResolveFieldContext<IHopexDataModel> ctx, IClassDescription entity)
        {
            var objectsToDelete = new List<IMegaObject>();

            string id = null;
            if (ctx.Arguments != null && ctx.Arguments.ContainsKey("id") && ctx.Arguments["id"].Value != null)
            {
                id = ctx.Arguments["id"].Value.ToString();
            }

            var idType = IdTypeEnum.INTERNAL;
            if (ctx.Arguments != null && ctx.Arguments.ContainsKey("idType") && ctx.Arguments["idType"].Value != null)
            {
                Enum.TryParse(ctx.Arguments["idType"].Value.ToString(), out idType);
            }

            switch (idType)
            {
                case IdTypeEnum.INTERNAL:
                    objectsToDelete.AddRange(root.GetSelection($"SELECT {entity.Id}[{entity.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{id}\"").ToList());
                    if (!objectsToDelete.Any())
                    {
                        throw new ExecutionError($"{entity.Name} with id: {id} not found.");
                    }
                    break;
                case IdTypeEnum.EXTERNAL:
                    objectsToDelete.AddRange(root.GetSelection($"SELECT {entity.Id}[{entity.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"").ToList());
                    if (!objectsToDelete.Any())
                    {
                        throw new ExecutionError($"{entity.Name} with external id: {id} not found.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            var isCascade = false;
            if (ctx.HasArgument("cascade"))
            {
                isCascade = (bool)ctx.Arguments["cascade"].Value;
            }

            return await ctx.Source.RemoveElementAsync(objectsToDelete, isCascade);
        }

        private async Task<DeleteResultType> DeleteManyElements(IMegaRoot root, IResolveFieldContext<IHopexDataModel> ctx, GraphQLClassDescription graphQLClass)
        {
            var objectsToDelete = new List<IModelElement>();

            if (ctx.Arguments.TryGetValue("filter", out var obj) && obj.Value is Dictionary<string, object>)
            {
                objectsToDelete.AddRange(await GetElements(root, ctx.Arguments, ctx.Source, ctx.Errors, graphQLClass));
            }

            var isCascade = false;
            if (ctx.Arguments.ContainsKey("cascade") && ctx.Arguments["cascade"].Value != null)
            {
                isCascade = (bool)ctx.Arguments["cascade"].Value;
            }

            return await ctx.Source.RemoveElementAsync(objectsToDelete, isCascade);
        }

        

        private void EnrichInterface(GenericObjectInterface genericObjectInterface)
        {
            var graphQlProperties = _genericClass.Properties.Select(p => new GraphQLPropertyDescription(p));
            CreateProperties<IModelElement>(graphQlProperties, genericObjectInterface, (ctx, prop) =>
            {
                string format = null;
                if (ctx.Arguments != null && ctx.Arguments.ContainsKey("format") && ctx.Arguments["format"].Value != null)
                {
                    format = ctx.Arguments["format"].ToString();
                }

                var result = ctx.Source.GetValue<object>(prop, ctx.Arguments, format);
                if (result is DateTime date && !string.IsNullOrEmpty(format))
                {
                    return date.ToString(format, CultureInfo.InvariantCulture);
                }

                return result;
            });
            CreateProperties<IModelElement>(graphQlProperties, genericObjectInterface.WildcardType,
                (ctx, prop) =>
                {
                    string format = null;
                    if (ctx.Arguments != null && ctx.Arguments.ContainsKey("format") && ctx.Arguments["format"].Value != null)
                    {
                        format = ctx.Arguments["format"].Value.ToString();
                    }

                    var result = ctx.Source.GetValue<object>(prop, ctx.Arguments, format);
                    if (result is DateTime date && !string.IsNullOrEmpty(format))
                    {
                        return date.ToString(format, CultureInfo.InvariantCulture);
                    }

                    return result;
                });
        }

        public static string ToValidName(string val)
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

        private static string RemoveDiacritics(string text)
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
