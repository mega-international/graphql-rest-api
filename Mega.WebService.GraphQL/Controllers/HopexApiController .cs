using System;
using System.IO;
using System.Text;
using System.Web.Http;
using System.Xml.Serialization;
using log4net;
using Mega.Bridge;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Filters;
using Mega.WebService.GraphQL.Models;

namespace Mega.WebService.GraphQL.Controllers
{
    [HopexAuthenticationFilter]
    public class HopexApiController : ApiController
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [HttpGet]
        [Route("environments")]
        public IHttpActionResult Environments()
        {
            var environmentsWithRepositoriesList = new HopexEnvironmentsWithRepositories();
            var adminBridge = new AdminBridge();

            var environmentsHopexResult = adminBridge.GetEnvironments();

            if (environmentsHopexResult.IsError)
            {
                const string errorMessage = "Error in Hopex environments results, see Hopex logs for more information";
                _logger.Error(errorMessage);
                return InternalServerError(new Exception(errorMessage));
            }

            using (var environmentsStreamReader = new MemoryStream(Encoding.UTF8.GetBytes(environmentsHopexResult.Result ?? "")))
            {
                var environmentsXmlSerializer = new XmlSerializer(typeof(HopexEnvironments));
                if (environmentsXmlSerializer.Deserialize(environmentsStreamReader) is HopexEnvironments environments)
                {
                    foreach (var environment in environments.Environments)
                    {
                        var environmentName = new DirectoryInfo(environment.Name).Name;
                        var environmentWithRepositories = new HopexEnvironmentWithRepositories
                        {
                            Id = environment.Id,
                            Name = environmentName,
                            //Path = environment.Name
                        };

                        var repositoriesHopexResult = adminBridge.GetRepositories(environment.Id);
                        if (repositoriesHopexResult.IsError)
                        {
                            const string errorMessage = "Error in Hopex repositories results, see Hopex logs for more information";
                            _logger.Error(errorMessage);
                            return InternalServerError(new Exception(errorMessage));
                        }

                        using (var repositoriesStreamReader = new MemoryStream(Encoding.UTF8.GetBytes(repositoriesHopexResult.Result ?? "")))
                        {
                            var repositoriesXmlSerializer = new XmlSerializer(typeof(HopexEnvironmentDatabases));
                            var repositories = repositoriesXmlSerializer.Deserialize(repositoriesStreamReader) as HopexEnvironmentDatabases;
                            environmentWithRepositories.Repositories = repositories?.Bases;
                        }

                        environmentsWithRepositoriesList.Environments.Add(environmentWithRepositories);
                    }

                    return Ok(environmentsWithRepositoriesList);
                }
            }

            return InternalServerError(new Exception("Environments not found"));
        }
    }
}
