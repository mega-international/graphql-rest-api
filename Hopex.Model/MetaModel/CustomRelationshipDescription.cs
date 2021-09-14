using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    internal class CustomRelationshipDescription : IFieldDescription
    {
        private readonly IClassDescription _classDescription = null;
        public CustomRelationshipDescription(IClassDescription classDescription)
        {
            _classDescription = classDescription;
        }

        public IEnumerable<ISetter> CreateSetters(object value)
        {
            foreach (var customRelationSetter in CustomRelationshipSetter.CreateSetters(value))
            {
                yield return customRelationSetter;
            }
        }
    }
}
