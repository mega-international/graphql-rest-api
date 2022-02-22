using Hopex.Model.Abstractions.DataModel;
using System;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{

    public interface IClassDescription : IElementWithProperties
    {
        IHopexMetaModel MetaModel { get; }
        string Name { get; }
        string Id { get; }
        string Description { get; }
        bool IsEntryPoint { get; }
        bool IsGeneric { get; }
        IEnumerable<IRelationshipDescription> Relationships { get; }
        void CloneProperties(IClassDescription clone);
        IEnumerable<ISetter> CreateCustomPropertySetters(object value);
        IEnumerable<ISetter> CreateCustomRelationshipSetters(object value);
        IRelationshipDescription GetRelationshipDescription(string roleName, bool throwExceptionIfNotExists = true);
        Type NativeType { get; }
    }
}
