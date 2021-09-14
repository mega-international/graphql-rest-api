namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IEnumDescription
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string InternalValue { get; }
        int Order { get; }
    }
}
