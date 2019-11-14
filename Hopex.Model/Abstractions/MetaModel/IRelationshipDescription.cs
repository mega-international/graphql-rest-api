using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IRelationshipDescription
    {
        IClassDescription ClassDescription { get; }

        string Name { get; }
        MegaId RoleId { get; }
        string Description { get; }
        IPathDescription[] Path { get; }
    }
}
