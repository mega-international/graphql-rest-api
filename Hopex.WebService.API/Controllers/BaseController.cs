using Hopex.Common.JsonMessages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.Controllers
{
    public class BaseController : Controller
    {
        protected virtual async Task<IActionResult> ConvertWebServiceResponseToStringContent(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.PartialContent:
                {
                    foreach (var header in response.Headers)
                    {
                        Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                    }
                    Response.Headers.Add("Content-Type", "application/json");
                    return StatusCode((int) HttpStatusCode.PartialContent, string.Empty);
                }
                case HttpStatusCode.OK:
                {
                    var getJobResult = await response.Content.ReadAsStringAsync();
                    Response.Headers.Add("Content-Type", "application/json");
                    return Ok(getJobResult);
                }
                default:
                {
                    Response.Headers.Add("Content-Type", "application/json");
                    return StatusCode((int) HttpStatusCode.PartialContent, string.Empty);
                }
            }
        }

        protected virtual async Task<IActionResult> ConvertWebServiceResponseToFileStream(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.PartialContent:
                {
                    foreach (var header in response.Headers)
                    {
                        Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                    }
                    Response.Headers.Add("Content-Type", "application/json");
                    return StatusCode((int) HttpStatusCode.PartialContent, string.Empty);
                }
                case HttpStatusCode.OK:
                {
                    var macroResponse = await response.Content.ReadAsStringAsync();
                    var fileResult = ProcessMacroResultToFile(macroResponse);
                    return fileResult;
                }
                default:
                {
                    Response.Headers.Add("Content-Type", "application/json");
                    return StatusCode((int) HttpStatusCode.PartialContent, string.Empty);
                }
            }
        }

        protected virtual IActionResult ProcessMacroResult(string macroResponse)
        {
            var errorMacroResponse = JsonConvert.DeserializeObject<ErrorMacroResponse>(macroResponse);
            if (errorMacroResponse?.Error != null)
            {
                return new ContentResult { Content = errorMacroResponse.Error, ContentType = "application/json", StatusCode = (int)errorMacroResponse.HttpStatusCode };
            }
            return new ContentResult { Content = macroResponse, ContentType = "application/json", StatusCode = 200 };
        }

        protected virtual IActionResult ProcessMacroResultToFile(string macroResponse)
        {
            var errorMacroResponse = JsonConvert.DeserializeObject<ErrorMacroResponse>(macroResponse);
            if (errorMacroResponse?.Error != null)
            {
                return new ContentResult { Content = errorMacroResponse.Error, ContentType = "application/json", StatusCode = (int)errorMacroResponse.HttpStatusCode };
            }
            var macroResult = JsonConvert.DeserializeObject<FileDownloadMacroResponse>(macroResponse);
            if (macroResult?.Content != null)
            {
                return File(new MemoryStream(Convert.FromBase64String(macroResult.Content)), macroResult.ContentType, macroResult.FileName);
            }
            return new ContentResult { StatusCode = 500 };
        }
    }
}
