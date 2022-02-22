namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPathDescription : IElementWithProperties
    {
        string RoleName { get; }
        string RoleId { get; }
        string Multiplicity { get; }
        IPathConditionDescription Condition { get; }
        string TargetSchemaId { get; }
        string TargetSchemaName { get; }
        IClassDescription TargetClass { get; }
    }
}
