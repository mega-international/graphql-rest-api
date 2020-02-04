using Hopex.Common.JsonMessages;
using Mega.Bridge.Models;
using Mega.Bridge.Services;
using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [RoutePrefix("api")]
    public class GraphQlController : BaseController
    {
        [HttpPost]
        [Route("{schemaName}")]
        public IHttpActionResult Execute(string schemaName, [FromBody] InputArguments args)
        {
            return Execute(schemaName, args, "");
        }

        [HttpPost]
        [Route("{version}/{schemaName}")]
        public IHttpActionResult Execute(string schemaName, [FromBody] InputArguments args, string version)
        {
            args.WebServiceUrl = Request.RequestUri.AbsoluteUri.Substring(0, Request.RequestUri.AbsoluteUri.IndexOf("/api/", StringComparison.Ordinal));

            HopexContext hopexContext = null;
            if (!TryParseHopexContext(ref hopexContext))
            {
                return BadRequest("Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}");
            }
            var headers = new Dictionary<string, string []>
            {
                {
                    "EnvironmentId", new[] {hopexContext.EnvironmentId}
                }
            };
            var path = BuildMacroPath("graphql", schemaName, version);
            var data = JsonConvert.SerializeObject(new
            {
                UserData = JsonConvert.SerializeObject(args),
                Request = new
                {
                    Headers = headers,
                    Path = path
                }
            });

            var result = CallMacro(GraphQlMacro, data);

            if(result.ErrorType == "None")
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<GraphQlResponse>(result.Content);
                    if(response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        return Content(response.HttpStatusCode, new { response.Error });
                    }
                    return Ok(JsonConvert.DeserializeObject(response.Result));
                }
                catch
                {
                    return InternalServerError(new Exception(result.Content));
                }
            }

            return FormatResult(result);
        }

        [HttpPost]
        [Route("async/{schemaName}")]
        public IHttpActionResult AsyncExecute(string schemaName, [FromBody] InputArguments args)
        {
            return AsyncExecute(schemaName, args, "");
        }

        [HttpPost]
        [Route("async/{version}/{schemaName}")]
        public IHttpActionResult AsyncExecute(string schemaName, [FromBody] InputArguments args, string version)
        {
            // Get values from x-hopex-wait
            var wait = TimeSpan.Zero;
            if(Request.Headers.TryGetValues("x-hopex-wait", out var hopexWait) && int.TryParse(hopexWait.FirstOrDefault(), out var hopexWaitMilliseconds))
            {
                wait = TimeSpan.FromMilliseconds(hopexWaitMilliseconds);
            }

            // Start job
            if(!Request.Headers.TryGetValues("x-hopex-task", out var hopexTask))
            {
                args.WebServiceUrl = Request.RequestUri.AbsoluteUri.Substring(0, Request.RequestUri.AbsoluteUri.IndexOf("/api/", StringComparison.Ordinal));

                HopexContext hopexContext = null;
                if (!TryParseHopexContext(ref hopexContext))
                {
                    return BadRequest("Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}");
                }
                var headers = new Dictionary<string, string []>
                {
                    {
                        "EnvironmentId", new[] {hopexContext.EnvironmentId}
                    }
                };
                var path = BuildMacroPath("graphql", schemaName, version);
                var data = JsonConvert.SerializeObject(new
                {
                    UserData = JsonConvert.SerializeObject(args),
                    Request = new
                    {
                        Headers = headers,
                        Path = path
                    }
                });

                return CallAsyncMacroExecute(GraphQlMacro, data, "MS", "RW", false, wait);
            }

            // Get Result
            return CallAsyncMacroGetResult(hopexTask.FirstOrDefault(), false, wait);
        }
        
        [HttpGet]
        [Route("{schemaName}/sdl")]
        public IHttpActionResult ExportSchema(string schemaName)
        {
            return ExportSchema(schemaName, "");
        }

        [HttpGet]
        [Route("{version}/{schemaName}/sdl")]
        public IHttpActionResult ExportSchema(string schemaName, string version)
        {
            HopexContext hopexContext = null;
            if (!TryParseHopexContext(ref hopexContext))
            {
                return BadRequest("Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}");
            }
            var headers = new Dictionary<string, string []>
            {
                {
                    "EnvironmentId", new[] {hopexContext.EnvironmentId}
                }
            };
            var path = BuildMacroPath("schema", schemaName, version);
            var data = new CallMacroArguments<object>(headers, path, null);

            var result = CallMacro(GraphQlMacro, data.ToString());

            return ProcessMacroResult(result, () =>
            {
                var response = JsonConvert.DeserializeObject<SchemaMacroResponse>(result.Content);
                var bytes = Encoding.UTF8.GetBytes(response.Schema);
                var contentStream = new StreamContent(new MemoryStream(bytes));
                var actionResult = new HttpResponseMessage(HttpStatusCode.OK) { Content = contentStream };
                actionResult.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                actionResult.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = $"{schemaName}.graphql" };
                return ResponseMessage(actionResult);
            });
        }

        protected virtual bool TryParseHopexContext(ref HopexContext hopexContext)
        {
            IEnumerable<string> hopexContextHeader;
            return Request.Headers.TryGetValues("x-hopex-context", out hopexContextHeader) && HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out hopexContext);
        }
        
        private static string BuildMacroPath(string route, string schemaName, string version)
        {
            var path = $"/api/{route}/{schemaName}";
            if(!string.IsNullOrEmpty(version))
            {
                path = $"/api/{route}/{version}/{schemaName}";
            }
            return path;
        }
    }
}
