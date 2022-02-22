using GraphQL;
using GraphQL.Execution;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;
using Mega.Macro.API.Enums;


namespace Hopex.Model.DataModel
{
    internal class HopexModelContext : IModelContext
    {
        public IModelElement TargetLinkAttributes { get; }
        public IEnumerable<IPropertyDescription> LinkAttributes { get; }
        public HopexModelContext(IModelElement targetLinkAttributes, IEnumerable<IPropertyDescription> linkAttributes)
        {
            TargetLinkAttributes = targetLinkAttributes;
            LinkAttributes = linkAttributes;
        }
    }

    internal class HopexModelElement : HasCollection, IModelElement
    {
        public MegaObject MegaObject { get; }
        public IHopexDataModel DomainModel { get; private set; }
        
        public IMegaObject IMegaObject { get; }

        private HopexModelElement _parent;
        public IModelElement Parent => _parent;

        //Context
        public HopexModelContext Context { get; internal set; }
        IModelContext IModelElement.Context => Context;
        //End context

        private readonly List<Exception> _errors = new List<Exception>();
        public IEnumerable<Exception> Errors { get => _errors; }

        public IMegaObject Language { get; set; }
        public MegaId Id { get; }
        public IClassDescription ClassDescription { get; }
        private Dictionary<string, IModelElement> TemporaryMegaObjects => DomainModel.TemporaryMegaObjects;


        public HopexModelElement(IHopexDataModel domainModel,
            IClassDescription schema,
            IMegaObject iMegaObject,
            MegaId id = null,
            IModelElement parent = null) : base(iMegaObject.Root)
        {
            DomainModel = domainModel;
            ClassDescription = schema;
            IMegaObject = iMegaObject;
            MegaObject = iMegaObject is RealMegaObject realObject ? realObject.RealObject : null;
            Id = InitializeId(IMegaObject, id);
            _parent = parent is HopexModelElement hopexParent ? hopexParent : null;
        }

        public void CreateContext(IModelElement targetLinkAttributes, IEnumerable<IPropertyDescription> linkAttributes)
        {
            Context = new HopexModelContext(targetLinkAttributes, linkAttributes);
        }

        public void SpreadContextFromParent()
        {
            Context = _parent.Context;
        }

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


        public IModelCollection GetGenericCollection(string collectionMegaId)
        {
            return new GenericModelCollection(collectionMegaId, this, DomainModel);
        }

        public override Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            var relationshipDescription = ClassDescription.GetRelationshipDescription(relationshipName ?? name);
            var collection = HopexModelCollection.Create(DomainModel, relationshipDescription, _iRoot, this, getCollectionArguments);
            return Task.FromResult(collection);
        }        

        public object GetGenericValue(string propertyMegaId, IDictionary<string, ArgumentValue> arguments)
        {
            string format;
            if (arguments.TryGetValue("format", out var formatArgument) && formatArgument.Value != null)
            {
                format = formatArgument.Value.ToString();
            }
            else
            {
                format = "ASCII";
            }
            var propertyDescription = new CustomPropertyDescription(propertyMegaId)
            {
                GetterFormat = format
            };
            var value = GetValue<string>(propertyDescription, arguments);
            return value;
        }

        public bool IsReadOnly(IPropertyDescription property)
        {
            var targetElement = GetTargetForProperty(property);
            var crud = targetElement.GetPropertyCrud(property);
            return crud.IsReadable && !crud.IsUpdatable && !crud.IsCreatable && !crud.IsDeletable;
        }

        public bool IsReadWrite(IPropertyDescription property)
        {
            var targetElement = GetTargetForProperty(property);
            var crud = targetElement.GetPropertyCrud(property);
            return crud.IsUpdatable;
        }

        public T GetValue<T>(IPropertyDescription property, IDictionary<string, ArgumentValue> arguments = null, string format = null)
        {
            CheckRelationship(property);
            var targetElement = GetTargetForProperty(property);
            var targetMegaObject = targetElement.IMegaObject;
            var permissions = targetElement.GetPropertyCrud(property);
            if (!permissions.IsReadable)
            {
                throw new ExecutionError($"You are not allowed to read the property ({property.Name}) on {ClassDescription.Name}: {MegaObject.MegaField}");
            }

            var propertyGetterFormat = format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat;
            
            T result;
            switch (property.PropertyType)
            {
                case PropertyType.Id:
                    result = targetMegaObject.GetPropertyValue<T>(property.Id, "External");
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
                        if(Enum.TryParse(arguments["nameSpace"].Value.ToString(), out NameSpaceFormatEnum nameSpaceFormatEnum))
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
                        var attribute = targetMegaObject.GetAttribute(propertyId);
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
                        var resultAsString = targetMegaObject.GetFormated(propertyId, format);
                        result = (T)Convert.ChangeType(resultAsString, typeof(T));
                        break;
                    }
                    result = targetMegaObject.GetPropertyValue<T>(propertyId, propertyGetterFormat);
                    break;
                case PropertyType.Enum:
                    var enumValue = targetMegaObject.GetPropertyValue<T>(property.Id);
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
                                enumResult = targetMegaObject.GetPropertyValue(property.Id, "Display");
                                break;
                            case EnumFormatEnum.ORDER:
                                enumResult = property.EnumValues.Where(x => x.InternalValue.ToString() == enumValueString).Select(x => x.Order + " - " + targetMegaObject.GetPropertyValue(property.Id, "Display")).FirstOrDefault();
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
                    var dateResult = targetMegaObject.GetPropertyValue<T>(property.Id, "");
                    if (dateResult.ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    if (propertyGetterFormat != "ASCII" && targetMegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    result = dateResult;
                    break;
                case PropertyType.Int:
                case PropertyType.Long:
                case PropertyType.Double:
                    var numericResult = targetMegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    if (numericResult.ToString() == "")
                    {
                        result = (T)Convert.ChangeType(null, typeof(T));
                        break;
                    }
                    if (propertyGetterFormat != "ASCII" && targetMegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
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
                    result = targetMegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
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
                            if(arguments.ContainsKey("dateRate") && arguments["dateRate"].Value != null && DateTime.TryParse(arguments["dateRate"].Value.ToString(), out DateTime dateRate))
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
                    if(DateTime.TryParse(arguments["dateRate"].Value.ToString(), out DateTime dateRate))
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
            var targetElement = GetTargetForProperty(property);
            var targetMegaObject = targetElement.IMegaObject;
            var permissions = targetElement.GetPropertyCrud(property);
            

            if (!permissions.IsUpdatable)
            {
                throw new ExecutionError($"You are not allowed to perform this action on this property ({property.Name})");
            }
            if (property.PropertyType == PropertyType.Id)
            {
                var id  = MegaId.Create(Utils.NormalizeHopexId(value.ToString()).Substring(0, 13));
                targetMegaObject.SetPropertyValue(property.Id, id.Value);
                MegaWrapperObject.IncCounter("MegaObject.SetProp");
            }
            else if (value is DateTime)
            {
                var dateTimeValue = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                targetMegaObject.SetPropertyValue(property.Id, dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            else
            {
                if (property.IsFormattedText)
                {
                    format = "HTML";
                }

                targetMegaObject.SetPropertyValue(property.Id, (object)value ?? "", format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);

                if(value != null && property.Id == MetaAttributeLibrary.ShortName.Substring(0, 13))
                {
                    var updatedName = targetMegaObject.GetPropertyValue(MetaAttributeLibrary.ShortName);
                    if (updatedName != null && updatedName != value.ToString())
                    {
                        if (string.Equals(updatedName, value.ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ExecutionError($@"Your object {{{targetMegaObject.MegaUnnamedField.Substring(0, 13)}}} named {{{value}}} has been automatically renamed to {{{updatedName}}}.");
                        }
                        throw new ExecutionError($@"Trying to set name {{{value}}} on object {{{targetMegaObject.MegaUnnamedField.Substring(0, 13)}}}, name as been automatically updated to {{{updatedName}}}.");
                    }
                }
            }
        }

        public async Task<IModelElement> UpdateAsync(IEnumerable<ISetter> setters)
        {
            var elementPermissions = GetCrud();
            if(!elementPermissions.IsUpdatable)
            {
                throw new ExecutionError($"You are not allowed to perform this action on this object ({MegaObject.MegaField})");
            }

            if (setters == null)
            {
                return this;
            }

            foreach (ISetter setter in setters)
            {
                try
                {
                    await setter.UpdateElementAsync(DomainModel, this);
                }
                catch(Exception ex)
                {
                    _errors.Add(ex);
                }                             
            }
            return this;
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
            return CrudComputer.GetPropertyCrud(IMegaObject, property);
        }

        private void CheckRelationship(IPropertyDescription property)
        {
            if(property.Scope == PropertyScope.Relationship && ( !(IMegaObject.Relationship?.Exists ?? false)))
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

        private IModelElement GetTargetForProperty(IPropertyDescription property)
        {
            if(property is CustomPropertyDescription)
            {
                return this;
            }

            if((ClassDescription?.IsGeneric ?? true) || ClassDescription.GetPropertyDescription(property.Name, false) != null)
            {
                return this;
            }

            if(Context?.LinkAttributes?.Any(att => att.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase)) ?? false)
            {
                return Context.TargetLinkAttributes;
            }

            throw new ExecutionError($"Cannot get target element for inexisting property {property.Name} in metaclass {ClassDescription.Name} from context {Context?.TargetLinkAttributes?.ClassDescription.Name ?? "root"}.");
        }

        public bool IsConfidential => IMegaObject.IsConfidential;

        public bool IsAvailable => IMegaObject.IsAvailable;

        public IModelElement BuildChildElement(IMegaObject megaObject, IRelationshipDescription relationship, int pathIdx)
        {
            var path = relationship.Path[pathIdx];
            var element = new HopexModelElement(DomainModel, path.TargetClass, megaObject, null, this);
            return ConnectChildElement(element, relationship, pathIdx);
        }

        private IModelElement ConnectChildElement(HopexModelElement child, IRelationshipDescription relationship, int pathIdx)
        {
            child._parent = this;
            if(pathIdx == 0)
            {
                var path = relationship.Path[pathIdx];
                var contextProperties = path.Properties?.Concat(path.TargetClass.Properties ?? new List<IPropertyDescription>()).ToList() ?? new List<IPropertyDescription>();
                child.CreateContext(child, contextProperties);
            }
            else
            {
                child.SpreadContextFromParent();
            }
            return child;
        }

        public async Task<IModelElement> GetElementByIdAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType)
        {
            var schema = relationship.TargetClass;
            return await GetElementByIdAsync(schema, relationship.Name, id, idType, DomainModel);
        }

        private async Task<IModelElement> LinkOrCreateElementAsync(IRelationshipDescription relationship, bool useInstanceCreator, object elementData, IEnumerable<ISetter> setters)
        {
            if(relationship is null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            IModelElement source = this;
            var createdElements = new List<IModelElement>();
            try
            {
                for(int idx = 0; idx < relationship.Path.Count(); ++idx)
                {
                    var isLast = idx == relationship.Path.Count() - 1;
                    var path = relationship.Path[idx];
                    if(!(relationship is CustomRelationshipDescription))
                    {
                        var permissions = CrudComputer.GetPathCrud(source.IMegaObject, path);
                        if(!permissions.IsCreatable)
                        {
                            throw new ExecutionError("You are not allowed to perform this action");
                        }
                    }
                    
                    var collection = source.IMegaObject.GetCollection(path.RoleId);

                    IModelElement element;
                    if(isLast)
                    {
                        if(elementData is Tuple<string, IdTypeEnum> pair) //C'est une création
                        {
                            var id = pair.Item1;
                            var idType = pair.Item2;
                            element = await CreateSingleElementAsync(path.TargetClass, useInstanceCreator, id, idType, collection,
                                mo => source.BuildChildElement(mo, relationship, idx), TemporaryMegaObjects, isLast);
                            createdElements.Add(element);
                        }
                        else if(elementData is IModelElement elementToLink) //C'est une connexion
                        {
                            if(elementToLink is HopexModelElement hopexElementToLink && source is HopexModelElement hopexSource)
                            {
                                hopexSource.ConnectChildElement(hopexElementToLink, relationship, idx);
                            }
                            element = elementToLink;
                            collection.Add(Utils.NormalizeHopexId(element.Id));
                        }
                        else
                        {
                            throw new ArgumentException($"elementData invalid: {elementData}");
                        }
                    }
                    else
                    {
                        element = await CreateSingleElementAsync(path.TargetClass, useInstanceCreator, null, IdTypeEnum.INTERNAL, collection,
                        mo => source.BuildChildElement(mo, relationship, idx), TemporaryMegaObjects, isLast);
                        createdElements.Add(element);
                    }
                    CreateCondition(element.IMegaObject, path.Condition);
                    source = element;
                }
            }
            catch(Exception)
            {
                foreach(var created in createdElements)
                {
                    created.IMegaObject.Delete();
                }
                throw;
            }
            return await source.UpdateAsync(setters);
        }

        public async Task<IModelElement> LinkElementAsync(IRelationshipDescription relationship, bool useInstanceCreator, IModelElement elementToLink, IEnumerable<ISetter> setters)
        {
            return await LinkOrCreateElementAsync(relationship, useInstanceCreator, elementToLink, setters);
        }

        public async Task<IModelElement> CreateElementAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
        {
            if(relationship is null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            if(!string.IsNullOrEmpty(id))
            {
                var existing = await DomainModel.GetElementByIdAsync(relationship.TargetClass, id, idType);
                if(existing != null)
                {
                    throw new ExecutionError($"Cannot create {relationship.TargetClass.Name} with identifier: {id}, this value is already used on element: {existing.IMegaObject.MegaField}");
                }
            }

            var elementData = new Tuple<string, IdTypeEnum>(id, idType);
            return await LinkOrCreateElementAsync(relationship, useInstanceCreator, elementData, setters);
        }

        private static void CreateCondition(IMegaObject current, IPathConditionDescription hopCondition)
        {
            if(hopCondition == null || string.IsNullOrEmpty(hopCondition.RoleId) || string.IsNullOrEmpty(hopCondition.ObjectFilterId))
            {
                return;
            }
            var conditionObject = current.Root.GetObjectFromId(Utils.NormalizeHopexId(hopCondition.ObjectFilterId));
            current.GetCollection(Utils.NormalizeHopexId(hopCondition.RoleId)).Add(conditionObject.Id);
        }
    }
}
