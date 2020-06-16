using Hopex.Model.Abstractions;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hopex.Model.PivotSchema.Loaders
{
    public class FileSystemLoader : IPivotSchemaLoader
    {
        private readonly string _basePath;
        private readonly IPivotSchemaLoader _fallbackLoader;

        public FileSystemLoader(string basePath, IPivotSchemaLoader fallbackLoader = null)
        {
            _basePath = basePath;
            _fallbackLoader = fallbackLoader;
        }

        public IEnumerable<SchemaReference> EnumerateStandardSchemas(string version)
        {
            if (_fallbackLoader != null)
            {
                foreach (var sr in _fallbackLoader.EnumerateStandardSchemas(version))
                {
                    yield return sr;
                }
            }
            else
            {
                var folder = Path.Combine(_basePath, version, "Standard");
                foreach (var file in Directory.GetFiles(folder, "*.json"))
                {
                    yield return new SchemaReference { Version = version, SchemaName = Path.GetFileNameWithoutExtension(file) };
                }
            }
        }

        public Task<Models.PivotSchema> ReadAsync(SchemaReference schemaRef)
        {
            string fileName = null;
            if (!schemaRef.IgnoreCustom && schemaRef.Version != null)
            {
                var customFolder = Path.Combine(_basePath, schemaRef.Version, "Custom");
                if (Directory.Exists(customFolder))
                {
                    fileName = Path.Combine(customFolder, $"{ schemaRef.EnvironmentId}_{ schemaRef.SchemaName}.json");
                    if (!File.Exists(fileName))
                    {
                        fileName = Path.Combine(customFolder, $"{ schemaRef.SchemaName}.json");
                        if (!File.Exists(fileName))
                        {
                            fileName = null;
                        }
                    }
                }
            }
            if (fileName == null)
            {
                fileName = Path.Combine(_basePath, schemaRef.Version, "Standard", schemaRef.SchemaName + ".json");
            }

            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                var pivot = JsonConvert.DeserializeObject<Models.PivotSchema>(json);
                pivot.Name = schemaRef.SchemaName;
                return Task.FromResult(pivot);
            }

            if (_fallbackLoader != null)
            {
                return _fallbackLoader.ReadAsync(schemaRef);
            }

            return Task.FromResult<Models.PivotSchema>(null);
        }
    }
}
