using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;
using Newtonsoft.Json;

namespace Hopex.Model.PivotSchema.Loaders
{
    public class ResourcesLoader : IPivotSchemaLoader
    {
        private readonly Assembly _assembly;
        private readonly string _namespace;
        private readonly string[] _names;

        public ResourcesLoader(Assembly assembly, string @namespace)
        {
            _assembly = assembly;
            _namespace = @namespace;
            _names = _assembly.GetManifestResourceNames();
        }

        public Task<Models.PivotSchema> ReadAsync(string schemaName)
        {
            var name = _names.FirstOrDefault(n => string.Compare(_namespace + "." + schemaName + ".json", n, true) == 0);
            if (name != null)
            {
                var stream = _assembly.GetManifestResourceStream(name);
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return Task.FromResult(JsonConvert.DeserializeObject<Models.PivotSchema>(reader.ReadToEnd()));
                    }
                }
            }
            return Task.FromResult<Models.PivotSchema>(null);
        }
    }
}
