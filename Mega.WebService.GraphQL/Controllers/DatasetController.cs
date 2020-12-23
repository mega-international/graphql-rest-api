using Hopex.Common.JsonMessages;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [RoutePrefix("api")]
    public class DatasetController : BaseController
    {
        [HttpGet]
        [Route("dataset/{datasetId}/content")]
        public IHttpActionResult GetContent(string datasetId)
        {
            return ProcessRequest(datasetId, data =>
            {
                var result = CallMacro(GraphQlMacro, data.ToString());
                return ProcessMacroResult(result, () =>
                {
                    var message = JsonConvert.DeserializeObject(result.Content);
                    return Ok(message);
                });
            });
        }

        [HttpGet]
        [Route("async/dataset/{datasetId}/content")]
        public IHttpActionResult AsyncGetContent(string datasetId)
        {
            // Start job
            if (!Request.Headers.TryGetValues("x-hopex-task", out var hopexTask))
            {
                return ProcessRequest(datasetId, data => CallAsyncMacroExecute(GraphQlMacro, data, "MS", "RW", false));
            }

            // Get Result
            return CallAsyncMacroGetResult(hopexTask.FirstOrDefault(), false);
        }

        private IHttpActionResult ProcessRequest(string datasetId, Func<string, IHttpActionResult> callMacro)
        {
            var userData = new DatasetArguments {
                Regenerate = Request.Headers.CacheControl?.NoCache ?? false
            };

            if (Request.Headers.TryGetValues("X-Hopex-NullValues", out IEnumerable<string> values))
                if (Enum.TryParse(values.First(), out DatasetNullValues parsedNullValues))                
                    userData.NullValues = parsedNullValues;            

            var data = new CallMacroArguments<DatasetArguments>($"/api/dataset/{datasetId}/content", userData);
            return callMacro(data.ToString());            
        }

        protected override IHttpActionResult BuildActionResultFrom(AsyncMacroResult macroResult)
        {
            var message = JsonConvert.DeserializeObject(macroResult.Result);
            return Ok(message);            
        }
    }
}
