using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Xml;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.HAS
{
    class RoleViewModel
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
        public string ModuleId { get; set; }
    }
    class RolesViewModel
    {
        public string IsAdministrator { get; set; } = "N";
        public List<RoleViewModel> Items { get; set; } = new List<RoleViewModel>();
    }
    class InputApiKeyViewModel
    {
        public int Id { get; set; }
        public string TokenName { get; set; }
        public string Description { get; set; }
        public string ExpirationDate { get; set; }
        public string Password { get; set; }
        public string EnvironmentId { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string AssignableElementId { get; set; }
        public string Login { get; set; }
        public string GuiLanguageId { get; set; }
        public string DataLanguageId { get; set; }
        public string ProfileId { get; set; }
        public string RepositoryId { get; set; }
        public string ConnectionMode { get; set; }
        public string SessionMode { get; set; }
        public string CanOpenSession { get; set; } = "N";
        public RolesViewModel Roles { get; internal set; } = new RolesViewModel();
    }

    internal class RepositoryJson
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    class RepositoryHAS : IRepository
    {
        private readonly ServerInfos _serverInfos;
        private readonly IEnvironment _environment;
        public string Id { get; private set; }
        public string Name { get; private set; }
        public RepositoryHAS(IEnvironment environment, RepositoryJson repositoryJson, ServerInfos serverInfos)
        {
            _environment = environment;
            _serverInfos = serverInfos;
            Id = repositoryJson.Id;
            Name = repositoryJson.Name;
        }

        public SessionDatas CreateSessionDatas(UserInfos userInfos, string profileId)
        {
            var apiKey = GenerateApiKey(userInfos, profileId);
            return new SessionDatas
            {
                ApiKey = apiKey
            };
        }

        public int Import(string ImportFileName, string RejectFileName, string Options)
        {
            throw new NotImplementedException();
        }

        private string GenerateApiKey(UserInfos userInfos, string profileId)
        {
            var uriCreateApiKey = new Uri($"{_serverInfos.Scheme}://{_serverInfos.Host}/uas/admin/apikey/create");
            var vm = new InputApiKeyViewModel
            {
                TokenName = "GraphQLIntegrationTestsKey",
                Description = "Tests key",
                CanOpenSession = "Y",
                UserName = userInfos.LoginName,
                EnvironmentId = _environment.Id,
                RepositoryId = Id,
                ProfileId = profileId,
                SessionMode = "MS",
                ConnectionMode = "RW",
                GuiLanguageId = "00(6wlHmk400",
                DataLanguageId = "00(6wlHmk400",
                AssignableElementId = userInfos.PersonId,
                UserId = userInfos.PersonId,
                Login = userInfos.LoginId,
                Roles = new RolesViewModel
                {
                    Items = new List<RoleViewModel>
                    {
                        new RoleViewModel
                        {
                            ModuleId = "has.console",
                            Name = "ClusterSettingsWriter"
                        },
                        new RoleViewModel
                        {
                            ModuleId = "has.console",
                            Name = "ModuleSettingsWriter"
                        }
                    }
                }
            };

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(vm));
                using (var httpResponse = httpClient.PostAsync(uriCreateApiKey, content).Result)
                {
                    httpResponse.EnsureSuccessStatusCode();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(httpResponse.Content.ReadAsStringAsync().Result);
                    var responseModel = JsonConvert.DeserializeObject<InputApiKeyViewModel>(httpResponse.Content.ReadAsStringAsync().Result);
                    return responseModel.Password;
                }
            }
        }
    }
}
