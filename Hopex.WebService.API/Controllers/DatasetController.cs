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
    [Route("api/dataset")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, BasicAuthScheme, Bearer, Cookies")]
    public class DatasetController : BaseController
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public DatasetController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpGet("{datasetId}/content")]
        public async Task<IActionResult> GetContent(string datasetId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}) enter.");
                var path = $"/api/dataset/{datasetId}/content";
                var userData = CreateDatasetArguments();
                var macroResponse = await _hopex.CallWebService<string>(path, userData);
                var contentResult = new ContentResult { Content = IncludeHeaders(macroResponse), ContentType = "application/json", StatusCode = 200 };
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}) leave.");
                return contentResult;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("async/dataset/{datasetId}/content")]
        public async Task<IActionResult> AsyncGetContent(string datasetId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncGetContent(datasetId: {datasetId}) enter.");
                var path = $"/api/dataset/{datasetId}/content";
                //If no x-hopex-task header, we want to start a new job
                if (!Request.Headers.TryGetValue("x-hopex-task", out var taskIds))
                {
                    var userData = CreateDatasetArguments();
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
                    _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}, start job) leave.");
                    return await ConvertWebServiceResponseToStringContent(startJobResponse);
                }
                //If x-hopex-task header has a value, we get the result of the job or partial content if the job is not finished or error if the task does not exist any more
                var getJobResponse = await _hopex.CallWebService(path, null, new Dictionary<string, string> { { "x-hopex-task", taskIds.FirstOrDefault() } });
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}, get job result) leave.");
                return await ConvertWebServiceResponseToStringContent(getJobResponse);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private string CreateDatasetArguments()
        {
            var datasetArguments = new DatasetArguments();
            if (Request.Headers.TryGetValue("cache-control", out var cacheControlValues))
            {
                if (cacheControlValues.FirstOrDefault() == "no-cache")
                {
                    datasetArguments.Regenerate = true;
                }
            }
            if (Request.Headers.TryGetValue("x-hopex-nullvalues", out var hopexNullValues))
            {
                if (Enum.TryParse(hopexNullValues.First(), out DatasetNullValues parsedNullValues))
                {
                    datasetArguments.NullValues = parsedNullValues;
                }
            }
            var userData = JsonConvert.SerializeObject(datasetArguments);
            return userData;
        }

        protected override async Task<IActionResult> ConvertWebServiceResponseToStringContent(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.PartialContent:
                    {
                        foreach (var header in response.Headers)
                        {
                            Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                        }
                        return StatusCode((int)HttpStatusCode.PartialContent);
                    }
                case HttpStatusCode.OK:
                    {
                        var getJobResult = await response.Content.ReadAsStringAsync();
                        return Ok(IncludeHeaders(getJobResult));
                    }
                default:
                    {
                        return StatusCode((int)HttpStatusCode.PartialContent);
                    }
            }
        }

        private string IncludeHeaders(string macroResponse)
        {
            if (Request.Headers.TryGetValue("include-header", out var includeHeaderValue) && bool.TryParse(includeHeaderValue, out var includeHeader) && !includeHeader)
            {
                var datasetMacroResponse = JsonConvert.DeserializeObject<DatasetMacroResponse>(macroResponse);
                var datasetMacroResponseJsonObject = JObject.FromObject(datasetMacroResponse);
                var datasetMacroResponseHeader = datasetMacroResponseJsonObject["header"]?.Parent;
                datasetMacroResponseHeader?.Remove();
                macroResponse = datasetMacroResponseJsonObject.ToString();
            }
            return macroResponse;
        }
    }
}
