using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{RoleName}->{TargetSchemaName}")]
    internal class PathDescription : IPathDescription
    {
        public PathDescription(string roleName, MegaId roleId)
        {
            RoleName = TargetSchemaName = roleName;
            RoleId = TargetSchemaId = Utils.NormalizeHopexId(roleId);
        }

        public PathDescription(string roleName, string roleId, string metaClassId, string metaClassName)
        {
            RoleName = roleName;
            RoleId = Utils.NormalizeHopexId(roleId);
            TargetSchemaId = Utils.NormalizeHopexId(metaClassId);
            TargetSchemaName = metaClassName;
        }

        public string RoleName { get; }
        public MegaId RoleId { get; }
        public string TargetSchemaName { get; }
        public MegaId TargetSchemaId { get; }
        public string Multiplicity { get; }
        public bool IsVisible { get; }
    }
}
