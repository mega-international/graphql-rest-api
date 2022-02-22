using GraphQL;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public class PropertySetter : ISetter
    {
        private readonly Exception _createError = null;
        private readonly IPropertyDescription _propertyDescription;
        private readonly object _value;
        public string SetterFormat { get; private set; }

        private PropertySetter(IPropertyDescription propertyDescription, object value)
        {
            _propertyDescription = propertyDescription;
            _value = value;
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
            return new PropertySetter(property, value)
            {
                SetterFormat = setterFormat
            };
        }

        public Task UpdateElementAsync(IHopexDataModel _, IModelElement element)
        {
            if(_createError != null)
            {
                throw _createError;
            }
            element.SetValue(_propertyDescription, _value, SetterFormat);
            return Task.CompletedTask;
        }
    }
}
