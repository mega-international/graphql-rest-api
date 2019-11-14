using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.MetaModel
{
    internal class EnumDescription : IEnumDescription
    {
        public EnumDescription(string name, string id, string description, string internalValue)
        {
            Name = name;
            Id = id;
            Description = description;
            InternalValue = internalValue;
        }

        public string Name { get; }
        public string Id { get; }
        public string Description { get; }
        public string InternalValue { get; }
    }
}
