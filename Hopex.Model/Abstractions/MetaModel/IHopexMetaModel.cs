using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IHopexMetaModel
    {
        IHopexMetaModel Inherits { get; }

        string Name { get; }

        IEnumerable<IClassDescription> Classes { get; }
        IEnumerable<IClassDescription> Interfaces { get; }
        IClassDescription FindClassDescriptionById(MegaId metaClassId);
        IClassDescription GetClassDescription(string schemaName, bool throwExceptionIfNotExists = true);
        IClassDescription GetInterfaceDescription(string schemaName, bool throwExceptionIfNotExists = true);
        IClassDescription GetGenericClass();
    }
}
