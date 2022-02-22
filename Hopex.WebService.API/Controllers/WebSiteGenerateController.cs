using HAS.Modules.WebService.API.Models.JsonMessages;
using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, BasicAuthScheme, Bearer, Cookies")]
    public class WebSiteGenerateController : BaseController
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public WebSiteGenerateController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpPost("async/generatewebsite")]
        public async Task<IActionResult> AsyncGenerateWebSite([FromBody] WebSiteArguments websiteArguments)
        {
            try
            {
                var userData = JsonConvert.SerializeObject(websiteArguments);
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncGetContent({userData}) enter.");
                var path = $"/api/generatewebsite";
                //If no x-hopex-task header, we want to start a new job
                if (!Request.Headers.TryGetValue("x-hopex-task", out var taskIds))
                {
                    HttpResponseMessage startJobResponse;
                    //If x-hopex-wait header has a value, we want to start a new job and wait for the macro to respond in x milliseconds or return the x-hopex-task in the response header
                    if (Request.Headers.TryGetValue("x-hopex-wait", out var hopexWait) && int.TryParse(hopexWait.FirstOrDefault(), out var hopexWaitMilliseconds))
                    {
                        startJobResponse = await _hopex.CallWebService(path, userData, new Dictionary<string, string> { { "x-hopex-wait", hopexWaitMilliseconds.ToString() } });
                    }
                    //If no x-hopex-wait header, we want to start a new job and wait for the macro to respond immediately or return the x-hopex-task in the response header
                    else
                    {
                        startJobResponse = await _hopex.CallWebService(path, userData, new Dictionary<string, string> { { "x-hopex-wait", "1" } });
                    }
                    _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent({userData}, start job) leave.");
                    return await ConvertWebServiceResponseToStringContent(startJobResponse);
                }
                //If x-hopex-task header has a value, we get the result of the job or partial content if the job is not finished or error if the task does not exist any more
                var getJobResponse = await _hopex.CallWebService(path, null, new Dictionary<string, string> { { "x-hopex-task", taskIds.FirstOrDefault() } });
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent({userData}, get job result) leave.");
                return await ConvertWebServiceResponseToStringContent(getJobResponse);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
