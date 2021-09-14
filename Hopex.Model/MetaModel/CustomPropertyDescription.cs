using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    internal class CustomPropertyDescription : IFieldDescription
    {
        private readonly IClassDescription _classDescription = null;
        public CustomPropertyDescription(IClassDescription classDescription)
        {
            _classDescription = classDescription;
        }
        public IEnumerable<ISetter> CreateSetters(object value)
        {
            foreach (var customPropertySetter in CustomFieldSetter.CreateSetters(value))
            {
                yield return customPropertySetter;
            }       
        }
    }
}
