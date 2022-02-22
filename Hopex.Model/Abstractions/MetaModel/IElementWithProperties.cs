using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IElementWithProperties
    {
        IEnumerable<IPropertyDescription> Properties { get; }
        void AddProperty(IPropertyDescription prop);
        IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true);
        IPropertyDescription FindPropertyDescriptionById(MegaId id);
    }
}
