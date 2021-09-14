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
                    return StatusCode((int) HttpStatusCode.PartialContent);
                }
                case HttpStatusCode.OK:
                {
                    var getJobResult = await response.Content.ReadAsStringAsync();
                    return Ok(getJobResult);
                }
                default:
                {
                    return StatusCode((int) HttpStatusCode.PartialContent);
                }
            }
        }

        protected async Task<IActionResult> ConvertWebServiceResponseToFileStream(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.PartialContent:
                {
                    foreach (var header in response.Headers)
                    {
                        Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                    }
                    return StatusCode((int) HttpStatusCode.PartialContent);
                }
                case HttpStatusCode.OK:
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var fileResponse = JsonConvert.DeserializeObject<FileDownloadMacroResponse>(content);
                    var fileResult = File(new MemoryStream(Convert.FromBase64String(fileResponse.Content)), fileResponse.ContentType, fileResponse.FileName);
                    return fileResult;
                }
                default:
                {
                    return StatusCode((int) HttpStatusCode.PartialContent);
                }
            }
        }
    }
}
