using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [RoutePrefix("api")]
    public class GraphQlController : BaseController
    {
        private const string GraphQlMacro = "AAC8AB1E5D25678E";

        [HttpPost]
        [Route("{schemaName}")]
        public IHttpActionResult Execute(string schemaName, [FromBody] InputArguments args)
        {
            var data = JsonConvert.SerializeObject(new
            {
                UserData = JsonConvert.SerializeObject(args),
                Request = new { Path = $"/api/graphql/{schemaName}" }
            });

            var result = CallMacro(GraphQlMacro, data);

            if(result.ErrorType == "None")
            {
                try
                {
                    var message = JsonConvert.DeserializeObject(result.Content);
                    return Ok(message);
                }
                catch
                {
                    return Ok(result.Content);
                }
            }

            return FormatResult(result);
        }

        [HttpPost]
        [Route("async/{schemaName}")]
        public IHttpActionResult AsyncExecute(string schemaName, [FromBody] InputArguments args)
        {
            // Get values from x-hopex-wait
            var wait = TimeSpan.Zero;
            if(Request.Headers.TryGetValues("x-hopex-wait", out var hopexWait) && int.TryParse(hopexWait.FirstOrDefault(), out var hopexWaitMilliseconds))
            {
                wait = TimeSpan.FromMilliseconds(hopexWaitMilliseconds);
            }
            // Start job
            if(!Request.Headers.TryGetValues("x-hopex-task", out var hopexTask))
            {
                var data = JsonConvert.SerializeObject(new
                {
                    UserData = JsonConvert.SerializeObject(args),
                    Request = new { Path = $"/api/graphql/{schemaName}" }
                });
                return CallAsyncMacroExecute(GraphQlMacro, data, "MS", "RW", false, wait);
            }
            // Get Result
            return CallAsyncMacroGetResult(hopexTask.FirstOrDefault(), false, wait);
        }

        private IHttpActionResult FormatResult(WebServiceResult result)
        {
            switch(result.ErrorType)
            {
                case "None":
                    return Ok(result.Content);
                case "BadRequest":
                    return BadRequest(result.Content);
                default:
                    return InternalServerError(new Exception($"{result.ErrorType}: {result.Content}"));
            }
        }
    }
}
