using System.Threading.Tasks;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.PivotSchema.Convertors;

namespace Hopex.Model.Abstractions
{

    public interface IPivotSchemaConvertor
    {
        /// <summary>
        /// Validate a pivot schema
        /// </summary>
        /// <param name="pivot"></param>
        /// <returns></returns>
        Task<ValidationContext> ValidateAsync(IHopexMetaModelManager schemaManager, PivotSchema.Models.PivotSchema pivot, SchemaReference origin);
        /// <summary>
        /// Convert a pivot schema to a Hopex meta model
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException">If schema is invalid</exception>
        Task<IHopexMetaModel> ConvertAsync(IHopexMetaModelManager schemaManager, PivotSchema.Models.PivotSchema schema, SchemaReference origin);
    }
}
