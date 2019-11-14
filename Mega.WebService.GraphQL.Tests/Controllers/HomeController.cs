using Mega.WebService.GraphQL.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Mega.WebService.GraphQL.Tests.Controllers
{
    public class HomeController : Controller
    {
        private IndexModel model;

        public HomeController()
        {
            model = new IndexModel();
        }
        public ActionResult Index()
        {
            model = new IndexModel();
            return View(model);
        }

        [System.Web.Services.WebMethod]
        public async Task<string> RunTestAsync(string testName, string testParams)
        {
            var result = await model?.RunTestAsync(testName, testParams);
            return JsonConvert.SerializeObject(result);
        }
    }
}
