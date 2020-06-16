using Hopex.Model.Abstractions.MetaModel;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    internal class RelationshipDescription : IRelationshipDescription
    {
        private readonly Dictionary<string, IPropertyDescription> _properties;

        public RelationshipDescription(string id, string reverseId, IClassDescription classDescription, string name, string roleId, string description)
        {
            Id = id;
            ClassDescription = classDescription;
            Name = name;
            RoleId = Utils.NormalizeHopexId(roleId);
            Description = description;
            ReverseId = reverseId;
            _properties = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
        }
        public string Id { get; }
        public string ReverseId { get; }
        public string Name { get; }
        public IPathDescription[] Path { get; private set; }
        public string Description { get; internal set; }
        public string RoleId { get; }
        public IClassDescription ClassDescription { get; }
        public IEnumerable<IPropertyDescription> Properties => _properties.Values;

        public IClassDescription TargetClass { get; internal set; }

        internal void SetPath(IEnumerable<IPathDescription> pathDescriptions)
        {
            Path = pathDescriptions.ToArray();
        }
    }
}
