using Mega.WebService.GraphQL.Tests.Models;
using Mega.WebService.GraphQL.Tests.Models.SessionDatas;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Mega.WebService.GraphQL.Tests.Sources.Tests.AbstractTest;

namespace Mega.WebService.GraphQL.Tests.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {}

        public ActionResult Index()
        {
            var model = new IndexModel();
            Session ["Environments"] = model.Environments;
            Session ["Tests"] = model.TestModels;
            Session ["Progression"] = new ProgressionModel();
            return View(model);
        }

        [System.Web.Services.WebMethod]
        public async Task<string> RunTestAsync(string testName, string testParams)
        {
            var progression = Session ["Progression"] as ProgressionModel;
            var testModels = Session ["Tests"] as Dictionary<string, TestModel>;
            var testModel = testModels[testName];
            var testParamsObj = JsonConvert.DeserializeObject<Parameters>(testParams);
            Session ["CurrentTest"] = testModel;
            var result = await testModel.Run(testParamsObj, progression).ConfigureAwait(false);
            Session ["CurrentTest"] = null;
            return JsonConvert.SerializeObject(result);
        }

        [System.Web.Services.WebMethod]
        public string GetRepos(string environmentId)
        {
            var environments = Session ["Environments"] as Dictionary<string, SessionEnvironment>;
            var environment = environments[environmentId];
            var result =  environment.Repositories;
            return JsonConvert.SerializeObject(result);
        }

        [System.Web.Services.WebMethod]
        public async Task<string> GetProgressionAsync()
        {
            var progression = Session["Progression"] as ProgressionModel;
            await progression.WaitForUpdate().ConfigureAwait(false);
            var result = JsonConvert.SerializeObject(progression);
            progression.Updated();
            return result;
        }
    }
}
