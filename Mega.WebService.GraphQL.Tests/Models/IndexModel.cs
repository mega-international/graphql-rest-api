using Mega.WebService.GraphQL.Tests.Models.SessionDatas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public class IndexModel
    {
        public readonly List<SelectListItem> TestSelectionList = new List<SelectListItem>();
        public readonly List<SelectListItem> EnvironmentSelectionList = new List<SelectListItem>();
        public readonly List<SelectListItem> RepositorySelectionList = new List<SelectListItem>();
        public readonly List<SelectListItem> ProfileSelectionList = new List<SelectListItem>();

        public readonly Dictionary<string, TestModel> TestModels = new Dictionary<string, TestModel>();
        public readonly Dictionary<string, SessionEnvironment> Environments = new Dictionary<string, SessionEnvironment>();
        
        public IndexModel()
        {
            GetTests();
            GetEnvironments();
            GetProfiles();
        }

        private void GetTests()
        {
            TestSelectionList.Clear();
            var strFileName = HttpContext.Current.Server.MapPath("~/App_Data/TestsList.json");
            var strModels = File.ReadAllText(strFileName);
            JArray jModels = JObject.Parse(strModels).GetValue("TestList") as JArray;
            foreach(JObject jModel in jModels)
            {
                TestModel model = jModel.ToObject<TestModel>();
                TestModels.Add(model.Name, model);
                TestSelectionList.Add(new SelectListItem
                {
                    Text = model.NameDisplay,
                    Value = JsonConvert.SerializeObject(model)
                });
            }
        }

        private List<SessionData> GetListByTag(string endPoint, string tagName)
        {
            var datas = new List<SessionData>();
            var xmlResponse = new XmlDocument();
            using(var client = new HttpClient())
            {
                var response = client.GetAsync(endPoint).Result;
                response.EnsureSuccessStatusCode();
                var strResponse = response.Content.ReadAsStringAsync().Result;
                xmlResponse.LoadXml(strResponse);
            }
            var root = xmlResponse.DocumentElement;
            if (root != null)
            {
                var xmlDatas = root.GetElementsByTagName(tagName);
                for (int idx = 0; idx < xmlDatas.Count; ++idx)
                {
                    var xmlData = xmlDatas.Item(idx);
                    var nodeId = xmlData.SelectSingleNode("Id");
                    var nodeName = xmlData.SelectSingleNode("Name");
                    var data = new SessionData(nodeId.InnerText, nodeName.InnerText);
                    datas.Add(data);
                }
            }
            return datas;
        }

        private void GetEnvironments()
        {
            EnvironmentSelectionList.Clear();
            RepositorySelectionList.Clear();

            var endPoint = ConfigurationManager.AppSettings["MegaSiteProvider"].TrimEnd('/') + "/env.mgsp?skey=";
            var tagName = "Environment";
            var sessionDatas = GetListByTag(endPoint, tagName);
            Environments.Clear();

            sessionDatas.ForEach(sessionData =>
            {
                var environment = new SessionEnvironment(sessionData);
                GetRepositories(environment);
                Environments.Add(environment.Id, environment);
                EnvironmentSelectionList.Add(new SelectListItem
                {
                    Text = environment.Name,
                    Value = environment.Id
                });
                if(EnvironmentSelectionList.Count == 1)
                {
                    var repositories = environment.Repositories;
                    repositories.ForEach(repository =>
                    {
                        RepositorySelectionList.Add(new SelectListItem
                        {
                            Text = repository.Name,
                            Value = repository.Id
                        });
                    });
                }
            });
        }

        private List<SessionRepository> GetRepositories(SessionEnvironment environment)
        {
            var endPoint = $"{ConfigurationManager.AppSettings["MegaSiteProvider"].TrimEnd('/')}/base.mgsp?skey=&env={environment.Id}";
            var tagName = "Base";
            var sessionDatas = GetListByTag(endPoint, tagName);
            var repositories = new List<SessionRepository>();
            sessionDatas.ForEach(sessionData => repositories.Add(new SessionRepository(sessionData, environment)));
            return repositories;
        }

        private void GetProfiles()
        {
            ProfileSelectionList.Clear();
            ProfileSelectionList.Add(new SelectListItem
            {
                Text = "HOPEX Customizer",
                Value = "757wuc(SGjpJ"
            });
        }
    }
}
