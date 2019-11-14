using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public class PropertySetter : ISetter
    {
        private PropertySetter()
        {
        }

        public static PropertySetter Create<T>(IPropertyDescription property, T value, string setterFormat = null)
        {
            return new PropertySetter
            {
                PropertyDescription = property,
                Value = value,
                SetterFormat = setterFormat
            };
        }

        public IPropertyDescription PropertyDescription { get; private set; }
        public virtual object Value { get; private set; }
        public string SetterFormat { get; private set; }

    }
}
