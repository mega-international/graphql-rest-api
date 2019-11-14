using System.Threading.Tasks;

namespace Hopex.Model.Abstractions
{
    public interface IPivotSchemaLoader
    {
        Task<PivotSchema.Models.PivotSchema> ReadAsync(string schemaName);
    }
}
