namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IRelationshipDescription : IFieldDescription
    {
        IClassDescription ClassDescription { get; }
        string Id { get; }
        string ReverseId { get; }
        string Name { get; }
        string RoleId { get; }
        string Description { get; }
        bool IsReadOnly { get; }
        IPathDescription[] Path { get; }
        IClassDescription TargetClass { get; }
    }
}
