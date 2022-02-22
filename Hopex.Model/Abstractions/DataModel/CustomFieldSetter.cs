using Hopex.Model.MetaModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    internal class CustomFieldSetter : ISetter
    {
        private readonly CustomPropertyDescription _property;
        private readonly object _value;

        private CustomFieldSetter(CustomPropertyDescription property, string value)
        {
            _property = property;
            _value = value;
        }

        public static IEnumerable<ISetter> CreateSetters(object values)
        {
            var props = (IEnumerable<object>)values;
            foreach (var prop in props)
            {
                var dict = (Dictionary<string, object>)prop;
                var propertyId = dict["id"].ToString();
                var value = dict["value"].ToString();
                var property = new CustomPropertyDescription(propertyId)
                {
                    SetterFormat = "ASCII"
                };
                yield return new CustomFieldSetter(property, value);
            }
        }

        public Task UpdateElementAsync(IHopexDataModel _, IModelElement element)
        {
            element.SetValue(_property, _value);
            return Task.CompletedTask;
        }
    }
}
