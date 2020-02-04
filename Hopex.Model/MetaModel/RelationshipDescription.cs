using System.Collections.Generic;
using System.Linq;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    internal class RelationshipDescription : IRelationshipDescription
    {
        public RelationshipDescription(string id, IClassDescription classDescription, string name, string roleId, string description)
        {
            Id = id;
            ClassDescription = classDescription;
            Name = name;
            RoleId = Utils.NormalizeHopexId(roleId);
            Description = description;
        }
        public string Id { get; }
        public string Name { get; }
        public IPathDescription[] Path { get; private set; }
        public string Description { get; internal set; }
        public string RoleId { get; }
        public IClassDescription ClassDescription { get; }

        internal void SetPath(IEnumerable<IPathDescription> pathDescriptions)
        {
            Path = pathDescriptions.ToArray();
        }
    }
}
