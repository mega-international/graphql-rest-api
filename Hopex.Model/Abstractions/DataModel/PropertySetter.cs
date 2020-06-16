using GraphQL;
using Hopex.Model.Abstractions.MetaModel;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public class PropertySetter : ISetter
    {
        private PropertySetter()
        {
        }

        public static PropertySetter Create<T>(IPropertyDescription property, T value, string setterFormat = null)
        {
            if (property.MaxLength != null)
            {
                var valueString = value as string;
                if (valueString != null && valueString.Length > property.MaxLength)
                    throw new ExecutionError($"Value {value} for {property.Name} exceeds maximum length of {property.MaxLength}");
            }
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

        public Task UpdateElementAsync(IHopexDataModel _, IModelElement element)
        {
            //_domainModel.LogInformation($"setter prop = {ps.PropertyDescription}");
            element.SetValue(PropertyDescription, Value, SetterFormat);
            return Task.CompletedTask;
        }
    }
}
