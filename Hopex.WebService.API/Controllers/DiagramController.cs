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
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, BasicAuthScheme, Bearer, Cookies")]
    public class DiagramController : BaseController
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public DiagramController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpGet("diagram/{diagramId}/image")]
        public async Task<IActionResult> GetImage(string diagramId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetImage(diagramId: {diagramId}) enter.");
                var path = $"/api/diagram/{diagramId}/image";
                if (!TryGetDiagramArgs(out var userData, out var actionResult))
                {
                    return actionResult;
                }
                var macroResponse = await _hopex.CallWebService<string>(path, userData);
                var fileResult = ProcessMacroResultToFile(macroResponse);
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetImage(diagramId: {diagramId}) leave.");
                return fileResult;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("async/diagram/{diagramId}/image")]
        public async Task<IActionResult> AsyncGetImage(string diagramId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncGetImage(diagramId: {diagramId}) enter.");
                var path = $"/api/diagram/{diagramId}/image";

                //If no x-hopex-task header, we want to start a new job
                if (!Request.Headers.TryGetValue("x-hopex-task", out var taskIds))
                {
                    if (!TryGetDiagramArgs(out var userData, out var actionResult))
                    {
                        return actionResult;
                    }
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
                    _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncExecute(diagramId: {diagramId}, start job) leave.");
                    return await ConvertWebServiceResponseToFileStream(startJobResponse);
                }
                //If x-hopex-task header has a value, we get the result of the job or partial content if the job is not finished or error if the task does not exist any more
                var getJobResponse = await _hopex.CallWebService(path, null, new Dictionary<string, string> { { "x-hopex-task", taskIds.FirstOrDefault() } });
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.AsyncExecute(diagramId: {diagramId}, get job result) leave.");
                return await ConvertWebServiceResponseToFileStream(getJobResponse);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private bool TryGetDiagramArgs(out string userData, out IActionResult actionResult)
        {
            var format = GetImageFormatFromHeader();
            if (!format.HasValue)
            {
                userData = null;
                actionResult = StatusCode((int)HttpStatusCode.NotAcceptable);
                return false;
            }
            var diagramArgs = new DiagramExportArguments
            {
                Format = format.Value
            };
            if (!TryParseQualityHeader(ref diagramArgs))
            {
                userData = null;
                actionResult = BadRequest("Invalid x-hopex-jpegquality, should a number between 1 and 100");
                return false;
            }
            userData = JsonConvert.SerializeObject(diagramArgs);
            actionResult = null;
            return true;
        }

        private ImageFormat? GetImageFormatFromHeader()
        {
            if (Request.Headers.TryGetValue("accept", out var acceptValues))
            {
                switch (acceptValues.FirstOrDefault())
                {
                    case "image/png":
                        return ImageFormat.Png;
                    case "image/jpeg":
                        return ImageFormat.Jpeg;
                    case "image/svg+xml":
                        return ImageFormat.Svg;
                }
            }
            return null;
        }

        private bool TryParseQualityHeader(ref DiagramExportArguments diagramArgs)
        {
            if (diagramArgs.Format == ImageFormat.Jpeg)
            {
                if (Request.Headers.TryGetValue("x-hopex-jpegquality", out var values))
                {
                    if (int.TryParse(values.First(), out var parsedQuality) && parsedQuality >= 1 && parsedQuality <= 100)
                    {
                        diagramArgs.Quality = (int)Math.Round(255f + 254f * (1 - parsedQuality) / 99f);
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
