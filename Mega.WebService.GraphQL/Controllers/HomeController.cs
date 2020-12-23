using Mega.WebService.GraphQL.Models;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Mega.WebService.GraphQL.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string id = "")
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["EnableTestingWebService"]))
            {
                return View("ServicesDisabled");
            }

            var schemas = ConfigurationManager.AppSettings["GraphQLSchemas"].Split(',').Select(x => x.Trim())
                .ToList();
            if (string.IsNullOrEmpty(id))
            {
                id = schemas[0];
            }

            var hopexInfo = new HopexInfo
            {
                UasUrl = ConfigurationManager.AppSettings["AuthenticationUrl"].TrimEnd('/') + "/connect/token/",
                ClientId = ConfigurationManager.AppSettings["ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                Scopes = ConfigurationManager.AppSettings["Scopes"],
                Schemas = new SelectList(schemas, id),
                EnvironmentId = ConfigurationManager.AppSettings["EnvironmentId"],
                RepositoryId = ConfigurationManager.AppSettings["RepositoryId"],
                ProfileId = ConfigurationManager.AppSettings["ProfileId"],
                Login = ConfigurationManager.AppSettings["Login"],
                Password = ConfigurationManager.AppSettings["Password"]
            };

            return View(hopexInfo);

        }

        public ActionResult Voyager(string id = "")
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["EnableTestingWebService"]))
            {
                return View("ServicesDisabled");
            }

            var schemas = ConfigurationManager.AppSettings ["GraphQLSchemas"].Split(',').Select(x => x.Trim()).ToList();
            if(string.IsNullOrEmpty(id))
            {
                id = schemas [0];
            }

            var hopexInfo = new HopexInfo
            {
                UasUrl = ConfigurationManager.AppSettings["AuthenticationUrl"].TrimEnd('/') + "/connect/token/",
                ClientId = ConfigurationManager.AppSettings["ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                Scopes = ConfigurationManager.AppSettings["Scopes"],
                Schemas = new SelectList(schemas, id),
                EnvironmentId = ConfigurationManager.AppSettings["EnvironmentId"],
                RepositoryId = ConfigurationManager.AppSettings["RepositoryId"],
                ProfileId = ConfigurationManager.AppSettings["ProfileId"],
                Login = ConfigurationManager.AppSettings["Login"],
                Password = ConfigurationManager.AppSettings["Password"]
            };

            return View(hopexInfo);
        }
    }
}
