using System;
using System.Collections.Generic;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IPropertyDescription
    {
        IClassDescription ClassDescription { get; }

        string Name { get; }
        MegaId Id { get; }
        string Description { get; }
        IEnumerable<IConstraintDescription> Constraints { get; }
        IEnumerable<IEnumDescription> EnumValues { get; }
        PropertyType PropertyType { get; }
        bool IsReadOnly { get; }
        bool IsFilterable { get; }
        bool IsRequired { get; }
        Type NativeType { get; }
        string SetterFormat { get; }
        string GetterFormat { get; }
    }
}
