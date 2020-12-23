using Hopex.Common;
using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hopex.WebService.API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, Bearer, Cookies")]
    public class GraphQlController : Controller
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;
        
        public GraphQlController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpPost("{schemaName}")]
        public async Task<IActionResult>  Execute(string schemaName, [FromBody] InputArguments graphQlRequest)
        {
            return await Execute(schemaName, graphQlRequest, string.Empty);
        }

        [HttpPost("{version}/{schemaName}")]
        public async Task<IActionResult>  Execute(string schemaName, [FromBody] InputArguments graphQlRequest, string version)
        {
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.Execute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) enter.");
            var path = string.IsNullOrEmpty(version) ? $"/api/graphql/{schemaName}" : $"/api/graphql/{version}/{schemaName}";
            var userData = JsonConvert.SerializeObject(graphQlRequest);
            var macroResponse = await _hopex.CallWebService<string>(path, userData);
            var contentResult = new ContentResult { Content = macroResponse, ContentType = "application/json", StatusCode = 200 };
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.Execute(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) leave.");
            return contentResult;
        }

        [HttpGet("{schemaName}/sdl")]
        public async Task<IActionResult> ExportSchema(string schemaName)
        {
            return await ExportSchema(schemaName, string.Empty);
        }

        [HttpGet("{version}/{schemaName}/sdl")]
        public async Task<IActionResult> ExportSchema(string schemaName, string version)
        {
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.ExportSchema(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) enter.");
            var path = string.IsNullOrEmpty(version) ? $"/api/schema/{schemaName}" : $"/api/schema/{version}/{schemaName}";
            var macroResponse = await _hopex.CallWebService<string>(path);
            var macroResult = JsonConvert.DeserializeObject<SchemaMacroResponse>(macroResponse);
            var fileResult = File(new MemoryStream(Encoding.UTF8.GetBytes(macroResult.Schema)), "text/plain", $"{schemaName}.graphql");
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.ExportSchema(schema: {schemaName}, version: {(string.IsNullOrEmpty(version) ? "Auto" : version)}) leave.");
            return fileResult;
        }
    }
}
