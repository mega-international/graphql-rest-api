using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.PivotSchema.Convertors;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IHopexMetaModelManager
    {
        IEnumerable<IHopexMetaModel> Schemas { get; }

        /// <summary>
        /// Get a meta model by name
        /// </summary>
        /// <param name="name">Meta model name</param>
        /// <returns></returns>
        Task<IHopexMetaModel> GetMetaModelAsync(SchemaReference schemaRef, ValidationContext ctx = null);
    }
}
