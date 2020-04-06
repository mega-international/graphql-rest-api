using GraphQL;
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


namespace Hopex.Model.DataModel
{
    internal class HopexModelElement : IModelElement, IDisposable
    {
        public MegaObject MegaObject { get; }
        public HopexDataModel DomainModel { get; private set; }

        public IMegaObject IMegaObject { get; }
        private IMegaRoot _iRoot;

        public HopexModelElement(HopexDataModel domainModel, IClassDescription schema, MegaObject megaObject, MegaId id = null)
        {
            DomainModel = domainModel;
            ClassDescription = schema;

            MegaObject = megaObject;
            _iRoot = RealMegaRootFactory.FromNativeRoot(megaObject.Root);
            IMegaObject = new RealMegaObject(megaObject);

            Id = InitializeId(IMegaObject, id);
        }

        public HopexModelElement(HopexDataModel domainModel, IClassDescription schema, IMegaObject iMegaObject, MegaId id = null)
        {
            DomainModel = domainModel;
            ClassDescription = schema;

            IMegaObject = iMegaObject;
            _iRoot = iMegaObject.Root;
            if (iMegaObject is RealMegaObject)
            {
                MegaObject = ((RealMegaObject)iMegaObject).RealObject;
            }            

            Id = InitializeId(IMegaObject, id);
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

        public MegaId Id { get; }
        public IClassDescription ClassDescription { get; }
        public IHopexMetaModel MetaModel => ClassDescription.MetaModel;


        public IModelCollection GetGenericCollection(string collectionMegaId)
        {
            return new GenericModelCollection(collectionMegaId, IMegaObject, DomainModel);
        }      

        public Task<IModelCollection> GetCollectionAsync(string name, string erql, List<Tuple<string, int>> orderByClauses = null, string relationshipName = null)
        {
            IRelationshipDescription relationshipDescription = ClassDescription.GetRelationshipDescription(relationshipName ?? name);
            return Task.FromResult<IModelCollection>(new HopexModelCollection(DomainModel, relationshipDescription, MegaObject.Root, _iRoot, MegaObject, erql, orderByClauses));
        }

        public object GetGenericValue(string propertyMegaId, Dictionary<string, object> arguments)
        {
            var normalizedId = Utils.NormalizeHopexId((string)arguments["id"]);
            if (! arguments.TryGetValue("format", out var format))
            {
                format = "ASCII";
            }
            var propertyDescription = new PropertyDescription(ClassDescription, propertyMegaId, normalizedId, "", "string", null, null)
            {
                GetterFormat = format.ToString()
            };
            var value = GetValue<string>(propertyDescription, arguments);
            return value;
        }

        public T GetValue<T>(string propertyName, Dictionary<string, object> arguments = null, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            return GetValue<T>(property, arguments, format);
        }

        public void SetValue<T>(string propertyName, T value, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            SetValue<T>(property, value, format);
        }

        public T GetValue<T>(IPropertyDescription property, Dictionary<string, object> arguments = null, string format = null)
        {
            var permissions = GetPropertyCrud(property);
            if (!permissions.IsReadable)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }

            var propertyGetterFormat = format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat;

            switch (property.PropertyType)
            {
                case PropertyType.Id:
                    return IMegaObject.GetPropertyValue<T>(property.Id, "External");
                case PropertyType.String:
                case PropertyType.RichText:
                    var propertyId = property.Id;
                    if (arguments != null && arguments.ContainsKey("nameSpace"))
                    {
                        NameSpaceFormatEnum nameSpaceFormatEnum;
                        if(Enum.TryParse(arguments["nameSpace"].ToString(), out nameSpaceFormatEnum))
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
                    var languageId = "";
                    if (arguments != null && arguments.ContainsKey("language"))
                    {
                        languageId = arguments["language"].ToString();
                    }
                    if (!string.IsNullOrEmpty(languageId))
                    {
                        var attribute = IMegaObject.GetAttribute(propertyId);
                        var translatedAttribute = attribute.Translate(languageId).Value();
                        return (T)Convert.ChangeType(translatedAttribute, typeof(T));
                    }
                    if (format != null)
                    {
                        if (format == "RAW")
                        {
                            format = "ANSI";
                        }
                        return MegaObject.NativeObject.GetFormated(propertyId, format);
                    }
                    if (property.IsFormattedText)
                    {
                        return MegaObject.NativeObject.GetFormated(propertyId, "HTML");
                    }
                    return IMegaObject.GetPropertyValue<T>(propertyId, propertyGetterFormat);
                case PropertyType.Enum:
                    var enumResult = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    var q = from x in property.EnumValues
                            where x.InternalValue == enumResult.ToString()
                            select x.Name;
                    return (T)Convert.ChangeType(q.FirstOrDefault(), typeof(T));
                case PropertyType.Date:
                    var dateResult = MegaObject.GetPropertyValue<T>(property.Id);
                    if (dateResult.ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    if (propertyGetterFormat != "ASCII" && MegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return dateResult;
                case PropertyType.Int:
                case PropertyType.Long:
                case PropertyType.Double:
                    var numericResult = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    if (numericResult.ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    if (propertyGetterFormat != "ASCII" && MegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return numericResult;
                case PropertyType.Currency:
                    var amountResult = FormatCurrency<T>(property, arguments, propertyGetterFormat);
                    if (amountResult == null)
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return amountResult;
                default:
                    return MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
            }
        }

        private T FormatCurrency<T>(IPropertyDescription property, IReadOnlyDictionary<string, object> arguments, string propertyGetterFormat)
        {
            object result = null;
            if (arguments != null && (arguments.ContainsKey("currency") || arguments.ContainsKey("dateRate")))
            {
                var currency = MegaObject.Root.CurrentEnvironment.Currency;
                var value = MegaObject.GetPropertyValue(property.Id);
                var currencyId = currency.GetCurrencyId(value);
                if (arguments.ContainsKey("currency"))
                {
                    var currencyCode = arguments["currency"].ToString();
                    var availableCurrency = MegaObject.Root.GetSelection($"Select {MetaClassLibrary.Currency} Where {MetaAttributeLibrary.CurrencyCode} = \"{currencyCode}\"").FirstOrDefault();
                    if (availableCurrency != null && availableCurrency.Exists)
                    {
                        if (MegaObject.Root.CurrentEnvironment.Toolkit.IsSameId(availableCurrency.Id, currencyId))
                        {
                            var currencyValue = MegaObject.NativeObject.GetAttribute(property.Id).Value;
                            result = currency.GetAmount(currencyValue);
                            DateTime dateRate;
                            if(arguments.ContainsKey("dateRate") && DateTime.TryParse(arguments["dateRate"].ToString(), out dateRate))
                            {
                                result = currency.NativeObject.GetInternalAmount(result, currencyId, currencyId, dateRate);
                            }
                        }
                        else
                        {
                            var availableCurrencyId = availableCurrency.GetPropertyValue(MetaAttributeLibrary.AbsoluteIdentifier);
                            var currencyValue = MegaObject.NativeObject.GetAttribute(property.Id).Translate(availableCurrencyId).Value;
                            result = currency.GetAmount(currencyValue);
                            DateTime dateRate;
                            if(arguments.ContainsKey("dateRate") && DateTime.TryParse(arguments["dateRate"].ToString(), out dateRate))
                            {
                                result = currency.NativeObject.GetInternalAmount(result, currencyId, availableCurrencyId, dateRate);
                            }
                        }
                    }
                }
                if (arguments.ContainsKey("dateRate"))
                {
                    var currencyValue = MegaObject.NativeObject.GetAttribute("~bKxT)KisHnbR[Amount]").Value;
                    result = currency.GetAmount(currencyValue);
                    DateTime dateRate;
                    if(DateTime.TryParse(arguments["dateRate"].ToString(), out dateRate))
                    {
                        result = currency.NativeObject.GetInternalAmount(result, currencyId, currencyId, dateRate);
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
            if (value is DateTime)
            {
                var dateTimeValue = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                MegaObject.SetPropertyValue(property.Id, dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            else
            {
                if (property.IsFormattedText)
                {
                    format = "HTML";
                }
                IMegaObject.SetPropertyValue(property.Id, value, format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);
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
                //_domainModel.LogInformation("before setter");
                await setter.UpdateElementAsync(DomainModel, this);                                   
                //_domainModel.LogInformation("after setter");
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
            return CrudComputer.GetPropertyCrud(IMegaObject, property);
        }
        
        public bool IsConfidential => IMegaObject.IsConfidential;

        public bool IsAvailable => IMegaObject.IsAvailable;
    }
}
