namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IEnumDescription
    {
        string Name { get; }
        string Id { get; }
        string Description { get; }
        string InternalValue { get; }
    }
}
