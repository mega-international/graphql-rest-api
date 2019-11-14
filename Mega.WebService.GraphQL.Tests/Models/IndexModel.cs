using Mega.WebService.GraphQL.Tests.Models.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public class TestModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameDisplay")]
        public string NameDisplay { get; set; }

        [JsonProperty("namespace")]
        public string NameSpace { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class IndexModel
    {
        public List<SelectListItem> TestSelectionList { get; set; } = new List<SelectListItem>();
        private readonly Dictionary<string, TestModel> TestModels = new Dictionary<string, TestModel>();

        public IndexModel()
        {
            string strFileName = HttpContext.Current.Server.MapPath("~/App_Data/TestsList.json");
            string strModels = File.ReadAllText(strFileName);
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

        public async Task<AbstractTest.Result> RunTestAsync(string testName, string testParams)
        {
            TestModel testModel = TestModels [testName];
            if(testModel == null)
            {
                throw new ArgumentException($"Test not found: {testName}");
            }

            AbstractTest.Parameters testParamsObj = JsonConvert.DeserializeObject<AbstractTest.Parameters>(testParams);
            //Choose selected test
            AbstractTest test = (AbstractTest)(Activator.CreateInstance(Type.GetType(testModel.NameSpace), testParamsObj));

            //Run test and return result
            return await Task.FromResult(test.Run(null));
        }
    }
}
