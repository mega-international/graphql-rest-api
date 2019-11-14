using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPathDescription
    {
        string RoleName { get; }
        MegaId RoleId { get; }
        string TargetSchemaName { get; }
        MegaId TargetSchemaId { get; }
        string Multiplicity { get; }
        bool IsVisible { get; }
    }
}
