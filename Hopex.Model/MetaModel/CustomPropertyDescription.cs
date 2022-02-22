using Hopex.Model.Abstractions.DataModel;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    internal class CustomPropertyDescription : PropertyDescription
    {
        public CustomPropertyDescription(string propId) : base(propId, Utils.NormalizeHopexId(propId), "", "string", null, null, null)
        {}

        public override IEnumerable<ISetter> CreateSetters(object value)
        {
            foreach (var customPropertySetter in CustomFieldSetter.CreateSetters(value))
            {
                yield return customPropertySetter;
            }       
        }
    }
}
