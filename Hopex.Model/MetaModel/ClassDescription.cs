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
        }

        public string Name { get; }
        public string Id { get; }
        public string Description { get; internal set; }
        private readonly Dictionary<string, IPropertyDescription> _properties;
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

        public void CloneProperties(IClassDescription clone)
        {
            foreach (PropertyDescription prop in _properties.Values)
            {
                PropertyDescription p = new PropertyDescription(this, prop.Name, prop.Id, prop.Description, prop.PropertyTypeName, prop.IsRequired, prop.IsReadOnly, prop.IsTranslatable, prop.IsFormattedText);
                foreach (IEnumDescription e in p.EnumValues)
                {
                    p.AddEnumValue(new EnumDescription(e.Name, e.Id, e.Description, e.InternalValue));
                }
                clone.AddProperty(p);
            }
        }

        public IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true)
        {
            if (propertyName == "id")
            {
                return new PropertyDescription(this, "id", MetaAttributeLibrary.AbsoluteIdentifier, "id", "string", true, true);
            }

            if (_properties.TryGetValue(propertyName, out IPropertyDescription cd))
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
        }


        internal IClassDescription Clone(IHopexMetaModel schema)
        {
            ClassDescription clone = new ClassDescription(schema, Name, Id, Description, IsEntryPoint);
            CloneProperties(clone);

            foreach (RelationshipDescription rel in _relationships.Values)
            {
                RelationshipDescription r = new RelationshipDescription(rel.Id, rel.ReverseId, this, rel.Name, rel.RoleId, rel.Description);
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
    }
}
