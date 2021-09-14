using System;
using System.Collections.Generic;
using Hopex.Model.Abstractions.DataModel;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.MetaModel
{

    public interface IClassDescription
    {
        IHopexMetaModel MetaModel { get; }
        string Name { get; }
        string Id { get; }
        string Description { get; }
        bool IsEntryPoint { get; }
        IEnumerable<IPropertyDescription> Properties { get; }
        IEnumerable<IPropertyDescription> PropertiesNotExtended { get; }
        IEnumerable<IRelationshipDescription> Relationships { get; }
        IEnumerable<IRelationshipDescription> RelationshipsNotExtended { get; }

        void AddProperty(IPropertyDescription prop);
        void CloneProperties(IClassDescription clone);
        IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true);

        IRelationshipDescription GetRelationshipDescription(string roleName, bool throwExceptionIfNotExists = true);

        Type NativeType { get; }
        IClassDescription Extends { get; }
        string GetBaseName();
        IPropertyDescription FindPropertyDescriptionById(MegaId id);
        bool IsPathProperty(IPropertyDescription prop);
        IEnumerable<ISetter> CreateSetter(IDictionary<string, object> arguments);
    }
}
