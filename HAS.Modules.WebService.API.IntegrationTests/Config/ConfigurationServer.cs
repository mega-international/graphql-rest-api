using Newtonsoft.Json;
using System.IO;

namespace HAS.Modules.WebService.API.IntegrationTests.Config
{
    public class ConfigurationServer
    {
        private class AppSettings
        {
            public string Server { get; set; }
            public string ApiKey { get; set; }
        }

        private readonly AppSettings _settings;
        public string Server => _settings.Server;
        public string ApiKey => _settings.ApiKey;
        public ConfigurationServer()
        {
            var filename = "Config/appsettings.local.json";
            if (!File.Exists(filename))
            {
                filename = "Config/appsettings.json";
            }
            var serializer = new JsonSerializer();
            using (var textReader = new StreamReader(filename))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    _settings = serializer.Deserialize<AppSettings>(jsonReader);
                }
            }
        }
    }
}
