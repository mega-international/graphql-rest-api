using System.Collections.Generic;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IModelCollection : IEnumerable<IModelElement>
    {
        IRelationshipDescription RelationshipDescription { get; }
    }
}
