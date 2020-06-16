using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IRelationshipDescription
    {
        IClassDescription ClassDescription { get; }
        string Id { get; }
        string ReverseId { get; }
        string Name { get; }
        string RoleId { get; }
        string Description { get; }
        IPathDescription[] Path { get; }
        IClassDescription TargetClass { get; }
    }
}
