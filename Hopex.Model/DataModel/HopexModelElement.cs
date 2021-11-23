using GraphQL;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;
using Mega.Macro.API.Enums;


namespace Hopex.Model.DataModel
{
    internal class HopexModelElement : IModelElement
    {
        public MegaObject MegaObject { get; }
        public IHopexDataModel DomainModel { get; private set; }
        
        public IMegaObject IMegaObject { get; }
        private IMegaRoot _iRoot;
        public IMegaObject Parent { get; private set; }

        private readonly List<Exception> _errors = new List<Exception>();
        public IEnumerable<Exception> Errors { get => _errors; }

        public IMegaObject Language { get; set; }

        public IModelElement PathElement { get; set; }

        private HopexModelElement(IHopexDataModel domainModel,
            IClassDescription schema,
            IMegaRoot iMegaRoot,
            IMegaObject iMegaObject,
            MegaObject megaObject,
            MegaId id,
            IMegaObject parent)
        {
            DomainModel = domainModel;
            ClassDescription = schema;
            IMegaObject = iMegaObject;
            MegaObject = megaObject;
            _iRoot = iMegaRoot;
            Id = InitializeId(IMegaObject, id);
            Parent = parent;
        }

        public HopexModelElement(IHopexDataModel domainModel, IClassDescription schema, MegaObject megaObject, MegaId id = null) :
            this(domainModel,
                schema,
                RealMegaRootFactory.FromNativeRoot(megaObject.Root),
                new RealMegaObject(megaObject),
                megaObject,
                id,
                null) {}

        public HopexModelElement(IHopexDataModel domainModel, IClassDescription schema, IMegaObject iMegaObject, MegaId id = null, IMegaObject parent = null) :
            this(domainModel,
                schema,
                iMegaObject.Root,
                iMegaObject,
                iMegaObject is RealMegaObject ? ((RealMegaObject)iMegaObject).RealObject : null,
                id,
                parent) {}

        private MegaId InitializeId(IMegaObject iMegaObject, MegaId id)
        {
            if (id == null)
            {
                id = Utils.SanitizeId(iMegaObject.MegaField);
                int pos = id.ToString().IndexOf('[');
                if (pos > 0)
                {
                    id = id.ToString().Substring(0, pos);
                }
            }
            return Utils.SanitizeId(id);
        }

        public MegaId Id { get; }
        public IClassDescription ClassDescription { get; }
        public IHopexMetaModel MetaModel => ClassDescription.MetaModel;


        public IModelCollection GetGenericCollection(string collectionMegaId)
        {
            return new GenericModelCollection(collectionMegaId, IMegaObject, DomainModel);
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            var relationshipDescription = ClassDescription.GetRelationshipDescription(relationshipName ?? name);
            var collection = HopexModelCollection.Create(DomainModel, relationshipDescription, _iRoot, IMegaObject, getCollectionArguments);
            return Task.FromResult(collection);
        }        

        public object GetGenericValue(string propertyMegaId, IDictionary<string, ArgumentValue> arguments)
        {
            var normalizedId = Utils.NormalizeHopexId(propertyMegaId);
            string format;
            if (arguments.TryGetValue("format", out var formatArgument) && formatArgument.Value != null)
            {
                format = formatArgument.Value.ToString();
            }
            else
            {
                format = "ASCII";
            }
            var propertyDescription = new PropertyDescription(ClassDescription, propertyMegaId, normalizedId, "", "string", null, null, null)
            {
                GetterFormat = format
            };
            var value = GetValue<string>(propertyDescription, arguments);
            return value;
        }

        public T GetValue<T>(string propertyName, IDictionary<string, ArgumentValue> arguments = null, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            return GetValue<T>(property, arguments, format);
        }

        public void SetValue<T>(string propertyName, T value, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            SetValue<T>(property, value, format);
        }

        public T GetValue<T>(IPropertyDescription property, IDictionary<string, ArgumentValue> arguments = null, string format = null)
        {
            CheckRelationship(property);
            var permissions = GetPropertyCrud(property);
            if (!permissions.IsReadable)
            {
                throw new ExecutionError($"You are not allowed to read the property ({property.Name}) on {ClassDescription.Name}: {MegaObject.MegaField}");
            }

            var propertyGetterFormat = format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat;

            //if (PropertyCache.TryGetValue<T>(out var cachedResult, IMegaObject.Id, property.Id, arguments, format))
            //{
            //    return cachedResult;
            //}

            T result;
            switch (property.PropertyType)
            {
                case PropertyType.Id:
                    result = IMegaObject.GetPropertyValue<T>(property.Id, "External");
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                    }
                    break;
                case PropertyType.String:
                case PropertyType.RichText:
                    var propertyId = property.Id;
                    if (arguments != null && arguments.ContainsKey("nameSpace") && arguments["nameSpace"].Value != null)
                    {
                        NameSpaceFormatEnum nameSpaceFormatEnum;
                        if(Enum.TryParse(arguments["nameSpace"].Value.ToString(), out nameSpaceFormatEnum))
                        {
                            switch (nameSpaceFormatEnum)
                            {
                                case NameSpaceFormatEnum.LOCAL:
                                    propertyId = MetaAttributeLibrary.GenericLocalName;
                                    break;
                                case NameSpaceFormatEnum.SHORT:
                                    propertyId = MetaAttributeLibrary.ShortName;
                                    break;
                                case NameSpaceFormatEnum.LONG:
                                    propertyId = MetaAttributeLibrary.Name;
                                    break;
                            }
                        }
                    }
                    var languageId = GetLanguage(arguments);
                    if(languageId != null)
                    {
                        var attribute = IMegaObject.GetAttribute(propertyId);
                        var translatedAttribute = attribute.Translate(languageId);
                        format = GetFormat(property, format);
                        if (format != null)
                        {
                            var resultAsString = translatedAttribute.GetFormatted(GetOutputFormat(format));
                            result = (T)Convert.ChangeType(resultAsString, typeof(T));
                            break;
                        }
                        result = (T)Convert.ChangeType(translatedAttribute.Value(), typeof(T));
                        break;
                    }
                    format = GetFormat(property, format);
                    if (format != null)
                    {
                        var resultAsString = MegaObject.GetFormatted(propertyId, format);
                        result = (T)Convert.ChangeType(resultAsString, typeof(T));
                        break;
                    }
                    result = IMegaObject.GetPropertyValue<T>(propertyId, propertyGetterFormat);
                    break;
                case PropertyType.Enum:
                    var enumValue = MegaObject.GetPropertyValue<T>(property.Id);
                    var enumValueString = enumValue.ToString();
                    object enumResult;
                    if (Enum.TryParse(propertyGetterFormat, out EnumFormatEnum enumFormat))
                    {
                        switch (enumFormat)
                        {
                            case EnumFormatEnum.SCHEMA_ID:
                                enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.Name).FirstOrDefault();
                                break;
                            case EnumFormatEnum.ID:
                                enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.Id).FirstOrDefault();
                                break;
                            case EnumFormatEnum.INTERNAL_VALUE:
                                enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.InternalValue).FirstOrDefault();
                                break;
                            case EnumFormatEnum.LABEL:
                                enumResult = MegaObject.GetPropertyValue(property.Id, "Display");
                                break;
                            case EnumFormatEnum.ORDER:
                                enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.Order + " - " + MegaObject.GetPropertyValue(property.Id, "Display")).FirstOrDefault();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.Name).FirstOrDefault();
                    }
                    result = (T)Convert.ChangeType(enumResult, typeof(T));
                    break;
                case PropertyType.Date:
                    var dateResult = IMegaObject.GetPropertyValue<T>(property.Id, "");
                    if (dateResult.ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    if (propertyGetterFormat != "ASCII" && IMegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    result = dateResult;
                    break;
                case PropertyType.Int:
                case PropertyType.Long:
                case PropertyType.Double:
                    var numericResult = IMegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    if (numericResult.ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    if (propertyGetterFormat != "ASCII" && IMegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    result = numericResult;
                    break;
                case PropertyType.Currency:
                    var amountResult = FormatCurrency<T>(property, arguments, propertyGetterFormat);
                    if (amountResult == null)
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    result = amountResult;
                    break;
                default:
                    result = IMegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    break;
            }
            //if (!PropertyCache.TryAdd(result, IMegaObject.Id, property.Id, arguments, format))
            //{
            //    Debug.Print($"PropertyCache.TryAdd failed for property: {IMegaObject.MegaField}.{property.Id}");
            //}
            return result;
        }

        private MegaId GetLanguage(IDictionary<string, ArgumentValue> arguments)
        {
            if (arguments != null && arguments.TryGetValue("language", out var languageValue) && languageValue.Value is IMegaObject language)
            {
                return language.Id;
            }
            return Language?.Id;
        }

        private static string GetFormat(IPropertyDescription property, string format)
        {
            if (format == null && property.IsFormattedText)
            {
                format = "HTML";
            }
            if (format is "RAW")
            {
                format = "ANSI";
            }
            return format;
        }

        private static OutputFormat GetOutputFormat(string format)
        {
            switch (format)
            {
                case "HTML":
                    return OutputFormat.Html;
                case "RTF":
                    return OutputFormat.Rtf;
                case "RAW":
                    return OutputFormat.Field;
                default:
                    return OutputFormat.Ansi;
            }
        }

        private T FormatCurrency<T>(IPropertyDescription property, IDictionary<string, ArgumentValue> arguments, string propertyGetterFormat)
        {
            object result = null;
            if (arguments != null && (arguments.ContainsKey("currency") || arguments.ContainsKey("dateRate")))
            {
                var currency = MegaObject.Root.CurrentEnvironment.Currency;
                var value = MegaObject.GetPropertyValue(property.Id);
                var currencyId = currency.GetCurrencyId(value);
                if (arguments.ContainsKey("currency") && arguments["currency"].Value != null)
                {
                    var currencyCode = arguments["currency"].Value.ToString();
                    var availableCurrency = MegaObject.Root.GetSelection($"Select {MetaClassLibrary.Currency} Where {MetaAttributeLibrary.CurrencyCode} = \"{currencyCode}\"").FirstOrDefault();
                    if (availableCurrency != null && availableCurrency.Exists)
                    {
                        if (MegaObject.Root.CurrentEnvironment.Toolkit.IsSameId(availableCurrency.Id, currencyId))
                        {
                            var currencyValue = MegaObject.GetAttribute(property.Id).GetValue().ToString();
                            result = currency.GetAmount(currencyValue);
                            DateTime dateRate;
                            if(arguments.ContainsKey("dateRate") && arguments["dateRate"].Value != null && DateTime.TryParse(arguments["dateRate"].Value.ToString(), out dateRate))
                            {
                                result = currency.GetInternalAmount((double)result, currencyId, currencyId, dateRate);
                            }
                        }
                        else
                        {
                            var availableCurrencyId = availableCurrency.GetPropertyValue(MetaAttributeLibrary.AbsoluteIdentifier);
                            var currencyValue = MegaObject.GetAttribute(property.Id).Translate(availableCurrencyId).GetValue().ToString();
                            result = currency.GetAmount(currencyValue);
                            DateTime dateRate;
                            if(arguments.ContainsKey("dateRate") && arguments["dateRate"].Value != null && DateTime.TryParse(arguments["dateRate"].Value.ToString(), out dateRate))
                            {
                                result = currency.GetInternalAmount((double)result, currencyId, availableCurrencyId, dateRate);
                            }
                        }
                    }
                }
                if (arguments.ContainsKey("dateRate") && arguments["dateRate"].Value != null)
                {
                    var currencyValue = MegaObject.GetAttribute("~bKxT)KisHnbR[Amount]").GetValue().ToString();
                    result = currency.GetAmount(currencyValue);
                    DateTime dateRate;
                    if(DateTime.TryParse(arguments["dateRate"].Value.ToString(), out dateRate))
                    {
                        result = currency.GetInternalAmount((double)result, currencyId, currencyId, dateRate);
                    }
                }
            }
            else
            {
                result = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
            }
            if (result is DBNull)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public void SetValue<T>(IPropertyDescription property, T value, string format = null)
        {
            var permissions = GetPropertyCrud(property);
            if (!permissions.IsUpdatable)
            {
                throw new ExecutionError($"You are not allowed to perform this action on this property ({property.Name})");
            }
            if (property.PropertyType == PropertyType.Id)
            {
                var id  = MegaId.Create(Utils.NormalizeHopexId(value.ToString()).Substring(0, 13));
                IMegaObject.SetPropertyValue(property.Id, id.Value);
                MegaWrapperObject.IncCounter("MegaObject.SetProp");
            }
            else if (value is DateTime)
            {
                var dateTimeValue = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                IMegaObject.SetPropertyValue(property.Id, dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            else
            {
                if (property.IsFormattedText)
                {
                    format = "HTML";
                }

                IMegaObject.SetPropertyValue(property.Id, (object)value ?? "", format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);

                if(value != null && property.Id == MetaAttributeLibrary.ShortName.Substring(0, 13))
                {
                    var updatedName = IMegaObject.GetPropertyValue(MetaAttributeLibrary.Name);
                    if (updatedName != null && updatedName != value.ToString())
                    {
                        throw new ExecutionError($@"An object {{{IMegaObject.MegaUnnamedField.Substring(0, 13)}}} named {{{value}}} already exists, name as been automatically updated to {{{updatedName}}}.");
                    }
                }
            }
        }

        internal async Task UpdateElement(IEnumerable<ISetter> setters)
        {
            if (setters == null)
            {
                return;
            }

            foreach (ISetter setter in setters)
            {
                try
                {
                    //if (setter.PropertyDescription?.Name.ToLower() == "name" /*&& setter.PropertyDescription?.IsUnique*/)
                    //{
                    //    var name = setter.Value.ToString();
                    //    var existingObjects = _iRoot.GetCollection(setter.PropertyDescription.Owner.Id).CallFunction("~nLn(jj)SCf30[LinkableQuery]", name, "ExactName=Yes, ListAll=Yes");
                    //    if (existingObjects != null && existingObjects.Count > 0)
                    //    {
                    //        _errors.Add(new Exception($@"Property name cannot be updated, an object named ""{name}"" already exists."));
                    //        continue;
                    //    }
                    //}
                    await setter.UpdateElementAsync(DomainModel, this);
                }
                catch(Exception ex)
                {
                    _errors.Add(ex);
                }                             
            }
        }                       
      
        public void Dispose()
        {
            DomainModel = null;
            (MegaObject as IDisposable)?.Dispose();
        }
                
        public CrudResult GetCrud()
        {
            return CrudComputer.GetCrud(IMegaObject);
        }

        public CrudResult GetPropertyCrud(IPropertyDescription property)
        {
            //if (PropertyCache.TryGetValue<CrudResult>(out var cachedResult, IMegaObject.Id, property.Id, cacheType: "CRUD"))
            //{
            //    return cachedResult;
            //}

            var result = CrudComputer.GetPropertyCrud(IMegaObject, property);

            //if (!PropertyCache.TryAdd(result, IMegaObject.Id, property.Id, cacheType: "CRUD"))
            //{
            //    Debug.Print($"PropertyCache.TryAdd failed for property: {IMegaObject.MegaField}.{property.Id}");
            //}

            return result;
        }

        private void CheckRelationship(IPropertyDescription property)
        {
            if(property.Scope == PropertyScope.Relationship && ( !(MegaObject.Relationship?.Exists ?? false)))
            {
                throw new ExecutionError($"Reading property {property.Name} from an inexisting relationship is forbidden.");
            }
        }

        public void AddErrors(IModelElement subElement)
        {
            foreach(var error in subElement.Errors)
            {
                _errors.Add(error);
            }
        }

        public bool IsConfidential => IMegaObject.IsConfidential;

        public bool IsAvailable => IMegaObject.IsAvailable;
    }
}
