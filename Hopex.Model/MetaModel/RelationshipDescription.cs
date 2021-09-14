using Hopex.Model.Abstractions.DataModel;
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

        public RelationshipDescription(string id, string reverseId, IClassDescription classDescription, string name, string roleId, string description, bool? isReadOnly)
        {
            Id = id;
            ClassDescription = classDescription;
            Name = name;
            RoleId = Utils.NormalizeHopexId(roleId);
            Description = string.IsNullOrEmpty(description) ? "No description" : description;
            ReverseId = reverseId;
            IsReadOnly = isReadOnly == true;
            _properties = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
        }
        public string Id { get; }
        public string ReverseId { get; }
        public string Name { get; }
        public IPathDescription[] Path { get; private set; }
        public string Description { get; internal set; }
        public string RoleId { get; }
        public bool IsReadOnly { get; internal set; }
        public IClassDescription ClassDescription { get; }
        public IEnumerable<IPropertyDescription> Properties => _properties.Values;

        public IClassDescription TargetClass { get; internal set; }

        public virtual IEnumerable<ISetter> CreateSetters(object value)
        {
            if (value is IDictionary<string, object> dict)
            {
                var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), dict["action"].ToString(), true);
                var list = (IEnumerable<object>)dict["list"];
                yield return CollectionSetter.Create(this, action, list);
            }
        }

        internal void SetPath(IEnumerable<IPathDescription> pathDescriptions)
        {
            Path = pathDescriptions.ToArray();
        }
    }
}
