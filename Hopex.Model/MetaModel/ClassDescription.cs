using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Library;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.MetaModel
{

    [System.Diagnostics.DebuggerDisplay("{Name}")]
    internal class ClassDescription : IClassDescription
    {

        private readonly Dictionary<string, IRelationshipDescription> _relationships;

        public ClassDescription(IHopexMetaModel schema, string name, string id, string description, bool isEntryPoint, IClassDescription extendedClass = null)
        {
            IsEntryPoint = isEntryPoint;
            Extends = extendedClass;
            MetaModel = schema;
            Name = name;
            Id = Utils.NormalizeHopexId(id);
            Description = description;
            _relationships = new Dictionary<string, IRelationshipDescription>(StringComparer.OrdinalIgnoreCase);
            _properties = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
            _propertiesByDisplay = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public string Id { get; }
        public string Description { get; internal set; }
        private readonly Dictionary<string, IPropertyDescription> _properties;
        private readonly Dictionary<string, IPropertyDescription> _propertiesByDisplay;

        public IEnumerable<IPropertyDescription> Properties
        {
            get
            {
                foreach (var p in _properties.Values)
                {
                    yield return p;
                }
                if (Extends != null)
                {
                    foreach (var p in Extends.Properties)
                    {
                        yield return p;
                    }
                }
            }
        }

        public IEnumerable<IPropertyDescription> PropertiesNotExtended
        {
            get
            {
                foreach (var p in _properties.Values)
                {
                    yield return p;
                }
            }
        }

        public void CloneProperties(IClassDescription clone)
        {
            foreach (PropertyDescription prop in _properties.Values)
            {
                PropertyDescription p = new PropertyDescription(this, prop.Name, prop.Id, prop.Description, prop.PropertyTypeName, prop.IsRequired, prop.IsReadOnly, prop.IsUnique, prop.IsTranslatable, prop.IsFormattedText);
                if (p.EnumValues != null)
                {
                    foreach (IEnumDescription e in p.EnumValues)
                    {
                        p.AddEnumValue(new EnumDescription(e.Name, e.Id, e.Description, e.InternalValue, e.Order));
                    }
                }
                clone.AddProperty(p);
            }
        }

        public IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true)
        {
            if (propertyName == "id")
            {
                return new PropertyDescription(this, "id", MetaAttributeLibrary.AbsoluteIdentifier, "id", "string", true, true, true);
            }

            if (_properties.TryGetValue(propertyName, out IPropertyDescription cd) || _propertiesByDisplay.TryGetValue(propertyName, out cd))
            {
                return cd;
            }

            if (Extends != null)
            {
                return Extends.GetPropertyDescription(propertyName, throwExceptionIfNotExists);
            }

            if (throwExceptionIfNotExists)
            {
                throw new Exception($"{propertyName} not found");
            }

            return null;
        }

        public void AddProperty(IPropertyDescription prop)
        {
            _properties.Add(prop.Name, prop);
            if(prop.Name != prop.DisplayName)
            {
                _propertiesByDisplay.Add(prop.DisplayName, prop);
            }
        }


        internal IClassDescription Clone(IHopexMetaModel schema)
        {
            ClassDescription clone = new ClassDescription(schema, Name, Id, Description, IsEntryPoint);
            CloneProperties(clone);

            foreach (RelationshipDescription rel in _relationships.Values)
            {
                RelationshipDescription r = new RelationshipDescription(rel.Id, rel.ReverseId, this, rel.Name, rel.RoleId, rel.Description, rel.IsReadOnly);
                r.SetPath(rel.Path);
                clone.AddRelationship(r);
            }
            return clone;
        }

        public bool IsEntryPoint { get; internal set; }
        public IClassDescription Extends { get; }

        public IEnumerable<IRelationshipDescription> Relationships
        {
            get
            {
                foreach (var p in _relationships.Values)
                {
                    yield return p;
                }
                if (Extends != null)
                {
                    foreach (var p in Extends.Relationships)
                    {
                        yield return p;
                    }
                }
            }
        }

        public IEnumerable<IRelationshipDescription> RelationshipsNotExtended
        {
            get
            {
                foreach (var p in _relationships.Values)
                {
                    yield return p;
                }
            }
        }

        public Type NativeType { get; }
        public IHopexMetaModel MetaModel { get; }

        public IRelationshipDescription GetRelationshipDescription(string roleName, bool throwExceptionIfNotExists = true)
        {
            if (_relationships.TryGetValue(roleName, out IRelationshipDescription cd))
            {
                return cd;
            }
            if (Extends != null)
            {
                return Extends.GetRelationshipDescription(roleName, throwExceptionIfNotExists);
            }
            if (throwExceptionIfNotExists)
            {
                throw new Exception($"{roleName} not found");
            }
            return null;
        }

        internal void AddRelationship(IRelationshipDescription rel)
        {
            _relationships.Add(rel.Name, rel);
        }

        public string GetBaseName() => Extends != null ? Extends.GetBaseName() : Name;

        public IPropertyDescription FindPropertyDescriptionById(MegaId id)
        {
            var normalizedId = Utils.NormalizeHopexId(id).ToString();
            return _properties.Values
                .Where(prop => prop.Id.Equals(normalizedId))
                .FirstOrDefault();
        }

        public bool IsPathProperty(IPropertyDescription prop)
        {
            return Extends != null && _properties.ContainsKey(prop.Name);
        }

        public IEnumerable<ISetter> CreateSetter(IDictionary<string, object> arguments)
        {
            foreach(var kv in arguments)
            {
                var propertyName = kv.Key;
                var value = kv.Value;
                IFieldDescription field;
                if (propertyName.Equals("customFields"))
                {
                    field = new CustomPropertyDescription(this);
                }
                else if (propertyName.Equals("customRelationships"))
                {
                    field = new CustomRelationshipDescription(this);
                }
                else if (propertyName == "dataLanguageCode")
                {
                    field = FindPropertyDescriptionById(MetaAttributeLibrary.DataLanguage.Substring(1, 12));
                    if (field != null)
                    {
                        value = ((IMegaObject)value).MegaUnnamedField.Substring(1, 12);
                    }
                    else
                    {
                        throw new Exception($"dataLanguage is not a valid member of {Name}");
                    }
                }
                else
                {
                    field = GetPropertyDescription(propertyName, false);
                    if(field == null)
                    {
                        field = GetRelationshipDescription(propertyName, false);
                    }
                }
                var setters = field?.CreateSetters(value) ?? throw new Exception($"{kv.Key} is not a valid member of {Name}");
                foreach (var setter in setters)
                {
                    yield return setter;
                }
            }
        }
    }
}
