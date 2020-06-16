namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPathConditionDescription
    {
        string RoleName { get; }
        string RoleId { get; }
        string MetaClassName { get; }
        string MetaClassId { get; }
        string Multiplicity { get; }
        string ObjectFilterId { get; }
        string ObjectFilterShortName { get; }
    }
}
