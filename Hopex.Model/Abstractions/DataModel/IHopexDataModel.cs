using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IHasCollection
    {
        Task<IModelCollection> GetCollectionAsync(string name, string erql = null, List<Tuple<string, int>> orderByClauses = null, string relationshipName = null);
        IHopexMetaModel MetaModel { get; }
    }

    public interface IHopexDataModel : IHasCollection
    {
        Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id);
        Task<IModelElement> CreateElementAsync(IClassDescription schema, IEnumerable<ISetter> setters, bool useInstanceCreator);
        Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IEnumerable<ISetter> setters);
        Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, bool cascade);
    }
}
