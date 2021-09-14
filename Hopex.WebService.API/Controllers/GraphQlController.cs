using Hopex.Common;
using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, BasicAuthScheme, Bearer, Cookies")]
    public class GraphQlController : BaseController
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public GraphQlController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpPost("{schemaName}")]
        public async Task<IActionResult> Execute(string schemaName, [FromBody] InputArguments graphQlRequest)
        {
            try
            {
                return await Execute(schemaName, graphQlRequest, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("{version}/{schemaName}")]
        public async Task<IActionResult> Execute(string schemaName, [FromBody] InputArguments graphQlRequest, string version)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.Execute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) enter.");
                var path = string.IsNullOrEmpty(version) ? $"/api/graphql/{schemaName}" : $"/api/graphql/{version}/{schemaName}";
                var userData = JsonConvert.SerializeObject(graphQlRequest);
                var macroResponse = await _hopex.CallWebService<string>(path, userData);
                var contentResult = new ContentResult { Content = macroResponse, ContentType = "application/json", StatusCode = 200 };
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.Execute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) leave.");
                return contentResult;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("async/{schemaName}")]
        public async Task<IActionResult> AsyncExecute(string schemaName, [FromBody] InputArguments graphQlRequest)
        {
            try
            {
                return await AsyncExecute(schemaName, graphQlRequest, "");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("async/{version}/{schemaName}")]
        public async Task<IActionResult> AsyncExecute(string schemaName, [FromBody] InputArguments graphQlRequest, string version)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncExecute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) enter.");
                var path = string.IsNullOrEmpty(version) ? $"/api/graphql/{schemaName}" : $"/api/graphql/{version}/{schemaName}";
                //If no x-hopex-task header, we want to start a new job
                if (!Request.Headers.TryGetValue("x-hopex-task", out var taskIds))
                {
                    var userData = JsonConvert.SerializeObject(graphQlRequest);
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
                    _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncExecute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}, start job) leave.");
                    return await ConvertWebServiceResponseToStringContent(startJobResponse);
                }
                //If x-hopex-task header has a value, we get the result of the job or partial content if the job is not finished or error if the task does not exist any more
                var getJobResponse = await _hopex.CallWebService(path, null, new Dictionary<string, string> { { "x-hopex-task", taskIds.FirstOrDefault() } });
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncExecute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}, get job result) leave.");
                return await ConvertWebServiceResponseToStringContent(getJobResponse);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("{schemaName}/sdl")]
        public async Task<IActionResult> ExportSchema(string schemaName)
        {
            try
            {
                return await ExportSchema(schemaName, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("{version}/{schemaName}/sdl")]
        public async Task<IActionResult> ExportSchema(string schemaName, string version)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.ExportSchema(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) enter.");
                var path = string.IsNullOrEmpty(version) ? $"/api/schema/{schemaName}" : $"/api/schema/{version}/{schemaName}";
                var macroResponse = await _hopex.CallWebService<string>(path);
                var macroResult = JsonConvert.DeserializeObject<SchemaMacroResponse>(macroResponse);
                var fileResult = File(new MemoryStream(Encoding.UTF8.GetBytes(macroResult.Schema)), "text/plain", $"{schemaName}.graphql");
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.ExportSchema(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) leave.");
                return fileResult;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
