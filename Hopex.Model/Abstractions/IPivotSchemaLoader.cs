using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions
{
    public class SchemaReference
    {
        public string SchemaName { get; set; }
        /// <summary>
        /// Schema version - null for standard
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Environment id (optional)
        /// </summary>
        public string EnvironmentId { get; set; }
        public string UniqueId => string.Concat(SchemaName, "-", Version);
        public bool IgnoreCustom { get; set; } = false;
    }

    public interface IPivotSchemaLoader
    {
        Task<PivotSchema.Models.PivotSchema> ReadAsync(SchemaReference schemaRef);
        IEnumerable<SchemaReference> EnumerateStandardSchemas(string version);

    }
}
