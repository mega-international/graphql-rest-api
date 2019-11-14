using System.Threading.Tasks;
using Hopex.Model.PivotSchema.Convertors;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IHopexMetaModelManager
    {
        /// <summary>
        /// Get a meta model by name
        /// </summary>
        /// <param name="name">Meta model name</param>
        /// <returns></returns>
        Task<IHopexMetaModel> GetMetaModelAsync(string name, ValidationContext ctx = null);
    }
}
