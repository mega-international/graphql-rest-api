using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.HAS
{
    internal class ApplicationHAS : IApplication
    {
        private const string _environmentName = "EnvTestLab";
        private readonly ServerInfos _serverInfos;
        public ApplicationHAS(ServerInfos serverInfos)
        {
            _serverInfos = serverInfos;
        }

        public IEnvironment GetEnvironment()
        {
            return GetEnvironmentByName(_environmentName);
        }

        public IHeaderBuilder InstanciateBuilder()
        {
            return new HeaderBuilderApiKey();
        }

        private IEnvironment GetEnvironmentByName(string environmentName)
        {
            var requestUri = _serverInfos.CreateUri("ssp/environments");
            string responseContent;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", _serverInfos.AdminKey);
                using (var response = httpClient.PostAsync(requestUri, new StringContent("{}")).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseContent = response.Content.ReadAsStringAsync().Result;
                }
            }
            var environmentNameLower = environmentName.ToLower();
            var environmentJson = JsonConvert.DeserializeObject<List<EnvironmentJson>>(responseContent).Find(env => env.Name.ToLower().Contains(environmentNameLower));
            return new EnvironmentHAS(_serverInfos, environmentJson);
        }
    }
}
