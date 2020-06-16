using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public class GetCollectionArguments
    {
        public string Erql { get; set; }
        public List<Tuple<string, int>> OrderByClauses { get; set; }
        public Func<IModelElement, bool> AdHocPredicate { get; set; }        
    }

    public interface IHasCollection
    {
        Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments);
        IHopexMetaModel MetaModel { get; }
    }

    public interface IHopexDataModel : IHasCollection
    {
        Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id, IdTypeEnum idType);
        Task<IModelElement> CreateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters);
        Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters);
        Task<IModelElement> CreateUpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters, bool useInstanceCreator);
        Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool cascade);
    }
}
