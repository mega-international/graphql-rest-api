using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IHopexMetaModel
    {
        IHopexMetaModel Inherits { get; }

        string Name { get; }

        IEnumerable<IClassDescription> Classes { get; }

        IClassDescription GetClassDescription(string schemaName, bool throwExceptionIfNotExists = true);
    }
}
