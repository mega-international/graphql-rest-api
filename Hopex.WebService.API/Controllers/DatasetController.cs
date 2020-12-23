using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hopex.WebService.API.Controllers
{
    [Route("api/dataset")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, Bearer, Cookies")]
    public class DatasetController : Controller
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
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}) enter.");
            var path = $"/api/dataset/{datasetId}/content";
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
            var macroResponse = await _hopex.CallWebService<string>(path, userData);
            var contentResult = new ContentResult { Content = macroResponse, ContentType = "application/json", StatusCode = 200 };
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetContent(datasetId: {datasetId}) leave.");
            return contentResult;
        }
    }
}
