using System.IO;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;
using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Loaders
{
    public class FileSystemLoader : IPivotSchemaLoader
    {
        private readonly string _basePath;

        public FileSystemLoader(string basePath)
        {
            _basePath = basePath;
        }

        public Task<Models.PivotSchema> ReadAsync(string schemaName)
        {
            string json = File.ReadAllText(Path.Combine(_basePath, schemaName + ".json"));
            var pivot = JsonConvert.DeserializeObject<Models.PivotSchema>(json);
            return Task.FromResult(pivot);
        }
    }
}
