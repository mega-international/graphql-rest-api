using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.WebService.API.Controllers
{
    [Route("api/diagram")]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, Bearer, Cookies")]
    public class DiagramController : Controller
    {
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public DiagramController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpGet("{diagramId}/image")]
        public async Task<IActionResult> GetImage(string diagramId)
        {
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetImage(diagramId: {diagramId}) enter.");
            var path = $"/api/diagram/{diagramId}/image";
            var format = GetImageFormatFromHeader();
            if (!format.HasValue)
            {
                return StatusCode((int) HttpStatusCode.NotAcceptable);
            }
            var diagramArgs = new DiagramExportArguments
            {
                Format = format.Value
            };
            if (!TryParseQualityHeader(ref diagramArgs))
            {
                return BadRequest("Invalid x-hopex-jpegquality, should a number between 1 and 100");
            }
            var userData = JsonConvert.SerializeObject(diagramArgs);
            var macroResponse = await _hopex.CallWebService<string>(path, userData);
            var macroResult = JsonConvert.DeserializeObject<FileDownloadMacroResponse>(macroResponse);
            var fileResult = File(new MemoryStream(Convert.FromBase64String(macroResult.Content)), macroResult.ContentType, macroResult.FileName);
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.GetImage(diagramId: {diagramId}) leave.");
            return fileResult;
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
