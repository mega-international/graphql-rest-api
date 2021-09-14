using GraphQL;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public class PropertySetter : ISetter
    {
        private PropertySetter()
        {
        }

        private PropertySetter(Exception createError)
        {
            _createError = createError;
        }

        public static PropertySetter Create(IPropertyDescription property, object value, string setterFormat = null)
        {
            if (property.MaxLength != null)
            {
                if (value is string valueString && valueString.Length > property.MaxLength)
                {
                    var error = new ExecutionError($"Value {value} for {property.Name} exceeds maximum length of {property.MaxLength}");
                    return new PropertySetter(error);
                }
            }
            return new PropertySetter
            {
                PropertyDescription = property,
                Value = value,
                SetterFormat = setterFormat
            };
        }

        public IPropertyDescription PropertyDescription { get; private set; }
        public object Value { get; private set; }
        public string SetterFormat { get; private set; }
        private readonly Exception _createError = null;

        public Task UpdateElementAsync(IHopexDataModel _, IModelElement element)
        {
            if(_createError != null)
            {
                throw _createError;
            }
            element.SetValue(PropertyDescription, Value, SetterFormat);
            return Task.CompletedTask;
        }
    }
}
