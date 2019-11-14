using System;
using System.Collections.Generic;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{

    public interface IClassDescription
    {
        IHopexMetaModel MetaModel { get; }
        string Name { get; }
        MegaId Id { get; }
        string Description { get; }
        bool IsEntryPoint { get; }
        IEnumerable<IPropertyDescription> Properties { get; }
        IEnumerable<IRelationshipDescription> Relationships { get; }

        IRelationshipDescription GetRelationshipDescription(string roleName, bool throwExceptionIfNotExists = true);

        Type NativeType { get; }

        IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true);
    }
}
