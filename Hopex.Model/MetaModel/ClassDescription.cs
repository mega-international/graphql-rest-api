using System;
using System.Collections.Generic;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Library;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    internal class ClassDescription : IClassDescription
    {
        private readonly Dictionary<string, IPropertyDescription> _properties;
        private readonly Dictionary<string, IRelationshipDescription> _relationships;

        public ClassDescription(IHopexMetaModel schema, string name, string id, string description, bool isEntryPoint)
        {
            IsEntryPoint = isEntryPoint;
            MetaModel = schema;
            Name = name;
            Id = Utils.NormalizeHopexId(id);
            Description = description;
            _properties = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
            _relationships = new Dictionary<string, IRelationshipDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public string Id { get; }
        public string Description { get; internal set; }

        internal IClassDescription Clone(IHopexMetaModel schema)
        {
            ClassDescription clone = new ClassDescription(schema, Name, Id, Description, IsEntryPoint);
            foreach (PropertyDescription prop in _properties.Values)
            {
                PropertyDescription p = new PropertyDescription(this, prop.Name, prop.Id, prop.Description, prop.PropertyTypeName, prop.IsRequired, prop.IsReadOnly, prop.IsTranslatable, prop.IsFormattedText);
                foreach (IEnumDescription e in p.EnumValues)
                {
                    p.AddEnumValue(new EnumDescription(e.Name, e.Id, e.Description, e.InternalValue));
                }
                clone.AddProperty(p);
            }

            foreach (RelationshipDescription rel in _relationships.Values)
            {
                RelationshipDescription r = new RelationshipDescription(rel.Id, this, rel.Name, rel.RoleId, rel.Description);
                r.SetPath(rel.Path);
                clone.AddRelationship(r);
            }
            return clone;
        }

        public bool IsEntryPoint { get; internal set; }
        public IEnumerable<IPropertyDescription> Properties => _properties.Values;
        public IEnumerable<IRelationshipDescription> Relationships => _relationships.Values;
        public Type NativeType { get; }
        public IHopexMetaModel MetaModel { get; }

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

            if (throwExceptionIfNotExists)
            {
                throw new Exception($"{propertyName} not found");
            }

            return null;
        }

        public IRelationshipDescription GetRelationshipDescription(string roleName, bool throwExceptionIfNotExists = true)
        {
            if (_relationships.TryGetValue(roleName, out IRelationshipDescription cd))
            {
                return cd;
            }

            if (throwExceptionIfNotExists)
            {
                throw new Exception($"{roleName} not found");
            }
            return null;
        }

        internal void AddProperty(IPropertyDescription prop)
        {
            _properties.Add(prop.Name, prop);
        }

        internal void AddRelationship(IRelationshipDescription rel)
        {
            _relationships.Add(rel.Name, rel);
        }
    }
}
