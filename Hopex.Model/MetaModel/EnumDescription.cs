using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.MetaModel
{
    internal class EnumDescription : IEnumDescription
    {
        public EnumDescription(string name, string id, string description, object internalValue, int order)
        {
            Id = id;
            Name = name;
            Description = description;
            InternalValue = internalValue;
            Order = order;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public object InternalValue { get; }
        public int Order { get; }
    }
}
