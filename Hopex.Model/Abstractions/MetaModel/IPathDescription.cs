using Hopex.Model.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPathDescription
    {
        string RoleName { get; }
        string RoleId { get; }
        string TargetSchemaName { get; }
        string TargetSchemaId { get; }
        string Multiplicity { get; }
        PathConditionDescription Condition { get; }
    }
}
