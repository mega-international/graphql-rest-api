using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IRelationshipDescription
    {
        IClassDescription ClassDescription { get; }
        string Id { get; }
        string Name { get; }
        string RoleId { get; }
        string Description { get; }
        IPathDescription[] Path { get; }
    }
}
