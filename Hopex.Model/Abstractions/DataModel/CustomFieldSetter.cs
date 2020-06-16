using Hopex.Model.MetaModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public class CustomFieldSetter : ISetter
    {
        public string PropertyId { get; internal set; }
        public string Value { get; internal set; }

        public CustomFieldSetter(string propertyId, string value)
        {
            PropertyId = propertyId;
            Value = value;
        }

        public static IEnumerable<ISetter> CreateSetters(object values)
        {
            var props = (List<object>)values;
            foreach (var prop in props)
            {
                var dict = (Dictionary<string, object>)prop;
                var propertyId = dict["id"].ToString();
                var value = dict["value"].ToString();
                yield return new CustomFieldSetter(propertyId, value);
            }
        }

        public Task UpdateElementAsync(IHopexDataModel _, IModelElement element)
        {
            var propertyDescription = new PropertyDescription(element.ClassDescription, PropertyId, PropertyId, "", "string", null, null)
            {
                SetterFormat = "ASCII"
            };
            element.SetValue(propertyDescription, Value);
            return Task.CompletedTask;
        }
    }
}
