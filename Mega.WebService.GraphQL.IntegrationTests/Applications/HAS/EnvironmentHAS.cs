using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.HAS
{
    internal class EnvironmentJson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<RepositoryJson> Repositories { get; set; }
    }

    internal class CreateRepositoryArgs
    {
        public string EnvId { get; set; }
        public string Name { get; set; }
    }

    internal class EnvironmentHAS : IEnvironment
    {
        private readonly ServerInfos _serverInfos;
        private readonly List<RepositoryHAS> _repositories = new List<RepositoryHAS>();
        public string Id { get; private set; }
        public string Path => "";

        public EnvironmentHAS(ServerInfos serverInfos, EnvironmentJson environmentJson)
        {
            _serverInfos = serverInfos;
            Id = environmentJson.Id;
            foreach(var repositoryJson in environmentJson.Repositories)
            {
                _repositories.Add(new RepositoryHAS(this, repositoryJson, serverInfos));
            }
        }

        public IRepository CreateRepository(string name, string options)
        {
            var repositoryJson = CreateRepositoryFromHAS(name);
            var newRepository = new RepositoryHAS(this, repositoryJson, _serverInfos);
            _repositories.Add(newRepository);
            return newRepository;
        }

        public IRepository GetRepositoryByName(string repositoryName)
        {
            return _repositories.Find(repo => string.Compare(repo.Name, repositoryName, true) == 0);
        }

        public void SetCurrentAdmin(string adminName, string password)
        {
        }

        private RepositoryJson CreateRepositoryFromHAS(string name)
        {
            var requestUri = _serverInfos.CreateUri("ssp/environment/repository/create");
            var args = new CreateRepositoryArgs
            {
                EnvId = Id,
                Name = name
            };
            var argsContent = JsonConvert.SerializeObject(args);
            string responseContent;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", _serverInfos.AdminKey);
                using (var response = httpClient.PostAsync(requestUri, new StringContent(argsContent)).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseContent = response.Content.ReadAsStringAsync().Result;
                }
            }
            return JsonConvert.DeserializeObject<RepositoryJson>(responseContent);
        }
    }
}
