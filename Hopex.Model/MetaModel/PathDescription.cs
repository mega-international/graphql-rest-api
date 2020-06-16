using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.PivotSchema.Models;
using Mega.Macro.API;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{RoleName}->{TargetSchemaName}")]
    internal class PathDescription : IPathDescription
    {
        public PathDescription(string roleName, string roleId)
        {
            RoleName = TargetSchemaName = roleName;
            RoleId = TargetSchemaId = Utils.NormalizeHopexId(roleId);
        }

        public PathDescription(string id, string roleName, string roleId, string metaClassId, string metaClassName, string multiplicity, PivotPathConditionDescription pathConditionDescription)
        {
            Id = id;
            RoleName = roleName;
            RoleId = Utils.NormalizeHopexId(roleId);
            TargetSchemaId = Utils.NormalizeHopexId(metaClassId);
            TargetSchemaName = metaClassName;
            Multiplicity = multiplicity;
            if (pathConditionDescription != null)
            {
                Condition = new PathConditionDescription(pathConditionDescription);
            }
        }
        public string Id { get; }
        public string RoleName { get; }
        public string RoleId { get; }
        public string TargetSchemaName { get; }
        public string TargetSchemaId { get; }
        public string Multiplicity { get; }
        public PathConditionDescription Condition { get; }
    }
}
