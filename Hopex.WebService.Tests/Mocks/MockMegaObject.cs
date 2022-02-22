using Hopex.Model.Abstractions;
using Hopex.WebService.Tests.Mocks.Drawings;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hopex.WebService.Tests.Mocks
{

    class MegaPropertyWithFormat
    {
        protected object _nativeValue;
        private Dictionary<string, object> _formattedValue = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        internal MegaPropertyWithFormat(object value)
        {
            _nativeValue = value;
        }

        internal MegaPropertyWithFormat WithDisplay(string displayValue)
        {
            _formattedValue.Add("Display", displayValue);
            return this;
        }

        internal MegaPropertyWithFormat WithAscii(string asciiValue)
        {
            _formattedValue.Add("ASCII", asciiValue);
            return this;
        }

        internal T GetPropertyValue<T>(string format)
        {
            if (_formattedValue.ContainsKey(format))
                return (T)_formattedValue[format];
            if (format.Equals("External", StringComparison.InvariantCultureIgnoreCase))
                return GetPropertyValueExternal<T>();
            return (T)_nativeValue;
        }

        private T GetPropertyValueExternal<T>()
        {
            return (T)(object)_nativeValue.ToString();
        }
    }

    [DebuggerDisplay("MockMegaObject {MegaField}")]
    public class MockMegaObject : MockMegaItem, IMegaObject
    {
        public IMegaDrawing _drawing;
        private readonly MegaId _classId;
        private readonly Dictionary<MegaId, object> _properties = new Dictionary<MegaId, object>(new MegaIdComparer());
        private readonly Dictionary<MegaId, MockMegaAttribute> _translatedProperties = new Dictionary<MegaId, MockMegaAttribute>(new MegaIdComparer());
        private readonly Dictionary<MegaId, MockMegaCollection> _links = new Dictionary<MegaId, MockMegaCollection>(new MegaIdComparer());
        private bool _isConfidential;

        public MegaId Id { get; internal set; }
        public MockMegaCollection Collection { get; set; } //Collection contextuelle dont provient l'objet si ce n'est pas la racine

        public virtual string MegaField => Id == null ? "~000000000000[<Unknown Mock Object>]" : MegaIdConverter.ToField(Id);

        public string MegaUnnamedField => MegaIdConverter.ToUnnamedField(Id);

        public virtual bool IsConfidential => _isConfidential;

        public bool IsAvailable => true;

        public virtual bool Exists => true;

        public IMegaObject Relationship => Collection?.Source;

        public MockMegaObject(MegaId id, MegaId classId = null)
        {
            Id = id;
            _classId = classId ?? MegaId.Create("000000000000");
            var id64 = id == null ? null : MegaIdConverter.To64(id);
            _properties ["~310000000D00[Absolute Identifier]"] = new MegaPropertyWithFormat(id64);
        }

        public MockMegaObject(string id)
            :this(MegaId.Create(id))
        {
        }

        public MockMegaObject(MockMegaObject toClone) : base(toClone)
        {
            Id = toClone.Id;
            Collection = toClone.Collection;

            _drawing = toClone._drawing;
            _classId = toClone._classId;
            _properties = toClone._properties;
            _translatedProperties = toClone._translatedProperties;
            _links = toClone._links;
            _isConfidential = toClone._isConfidential;
        }

        public MockMegaObject()
        { }

        public MockMegaObject WithDrawing(MockMegaDrawing drawing)
        {
            drawing._diagramObject = this;
            _drawing = drawing;
            return this;
        }

        internal MockMegaObject WithProperty(MegaId propId, object value)
        {
            _properties.Add(propId, value);
            return this;
        }

        internal MockMegaObject WithTranslatedProperty(MegaId propId, object defaultValue, Dictionary<MegaId, object> translatedValues)
        {
            var properlyIndexedValues = new Dictionary<MegaId, object>(translatedValues, new MegaIdComparer());            
            _properties.Add(propId, defaultValue);
            _translatedProperties.Add(propId, new MockMegaAttribute(defaultValue, properlyIndexedValues));
            return this;
        }

        internal MockMegaObject WithFormattedProperty(MegaId propId, MegaPropertyWithFormat value)
        {
            _properties.Add(propId, value);
            return this;
        }

        internal new MockMegaObject WithTypeObject(MockMegaObject typeObject)
        {
            base.WithTypeObject(typeObject);
            return this;
        }

        internal void PropagateRoot(MockMegaRoot root, MockMegaRoot.Builder builder)
        {
            Root = root;
            foreach (var pair in _links)
                pair.Value.PopagateRoot(root, builder);
        }

        internal MockMegaObject WithRelation(MockMegaCollection col)
        {
            AddLink(col);
            return this;
        }

        private void AddLink(MockMegaCollection col)
        {
            col.Source = this;
            _links.Add(col.Id, col);
        }

        internal MockMegaObject WithConfidentiality()
        {
            _isConfidential = true;
            return this;
        }

        public virtual IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null, int sortDirection3 = 1, string sortAttribute3 = null)
        {
            MegaIdUtils.EnsureValidPropertyId(linkId, "MegaObject.GetCollection");
            if(!_links.TryGetValue(linkId, out var collection))
            {
                collection = new MockMegaCollection(linkId) { Root = this.Root };
                AddLink(collection);
            }
            return collection;
        }

        public virtual MegaId GetClassId()
        {
            return _classId;
        }
        
        public virtual string GetPropertyValue(MegaId propertyId, string format = "ASCII")
        {
            return GetPropertyValueInternal<string>(propertyId, format);
        }

        public virtual T GetPropertyValue<T>(MegaId propertyId, string format = "internal")
        {
            return GetPropertyValueInternal<T>(propertyId, format);
        }

        public dynamic GetFormated(string propertyId, string format)
        {
            return GetPropertyValueInternal<object>(propertyId, format);
        }

        private T GetPropertyValueInternal<T>(MegaId propertyId, string format)
        {
            MegaIdUtils.EnsureValidPropertyId(propertyId, "MegaObject.GetPropertyValue");
            if (_properties.ContainsKey(propertyId))
            {
                var value = _properties[propertyId];
                if (value is MegaPropertyWithFormat)
                {
                    return ((MegaPropertyWithFormat) value).GetPropertyValue<T>(format);
                }
                return (T) _properties[propertyId];
            }
            return default(T);
        }      

        public void SetPropertyValue(MegaId propertyId, string value)
        {
            _properties[propertyId] = value;
        }

        public void SetPropertyValue(MegaId propertyId, object value, string format = "internal")
        {
            _properties[propertyId] = value;
        }

        public IMegaAttribute GetAttribute(MegaId propertyId)
        {
            MegaIdUtils.EnsureValidPropertyId(propertyId, "MegaObject.GetAttribute");
            return _translatedProperties[propertyId];
        }

        public bool IsSameId(MegaId objectId)
        {
            return new MegaIdComparer().Compare(Id, objectId) == 0;
        }

        public void Delete(string options = "")
        {
            throw new NotImplementedException();
        }

        public void CallMethod(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            throw new NotImplementedException();
        }

        public IMegaObject GetPhysicalType()
        {
            return this;
        }

        public MockMegaObject FromCollection(MockMegaCollection collection)
        {
            Collection = collection;
            return this;
        }

        public MockMegaObject FromRoot()
        {
            Collection = null;
            return this;
        }
    }

    public class InexistingMockMegaObject : MockMegaObject
    {
        public override bool Exists => false;

        public override string MegaField => "~000000000000[<Empty Object>]";
    }
}
