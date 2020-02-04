using System;
using System.Collections.Generic;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPropertyDescription
    {
        IClassDescription ClassDescription { get; }

        string Name { get; }
        string Id { get; }
        string Description { get; }
        IEnumerable<IConstraintDescription> Constraints { get; }
        IEnumerable<IEnumDescription> EnumValues { get; }
        PropertyType PropertyType { get; }
        bool IsReadOnly { get; }
        bool IsRequired { get; }
        bool IsTranslatable { get; }
        bool IsFormattedText { get; }
        int? MaxLength { get; }
        Type NativeType { get; }
        string SetterFormat { get; }
        string GetterFormat { get; }
    }
}
