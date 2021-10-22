using Hopex.Common;
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
        [Route("test")]
        public IHttpActionResult TestExecute([FromBody] InputArguments args)
        {
            var headers = new Dictionary<string, string []>
            {
                {
                    "EnvironmentId", new[] {Request.Headers.GetValues("x-hopex-environment-id").FirstOrDefault()}
                }
            };
            var data = new CallMacroArguments<object>(headers, "/api/test", args);
            var macroResult = CallMacro(GraphQlMacro, data.ToString());
            var response = JsonConvert.DeserializeObject<GraphQlResponse>(macroResult.Content);
            return Ok(JsonConvert.DeserializeObject(response.Result));
        }

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
            Logger.Info($"GraphQlController.Execute(\"{schemaName}\", \"{version}\") enter.");
            CompleteHeadersFromWebConfig();
            var result = ProcessQueryRequest(schemaName, args, version, data =>
            {
                var macroResult = CallMacro(GraphQlMacro, data);

                if (macroResult.ErrorType == "None")
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<GraphQlResponse>(macroResult.Content);
                        if (response.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return Content(response.HttpStatusCode, new { response.Error });
                        }
                        return Ok(JsonConvert.DeserializeObject(response.Result));
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(macroResult.Content, ex);
                        return InternalServerError(new Exception(macroResult.Content));
                    }
                }
                Logger.Info($"GraphQlController.Execute(\"{schemaName}\", \"{version}\") leave.");
                return FormatResult(macroResult);
            });
            return result;
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
            Logger.Info($"GraphQlController.AsyncExecute(\"{schemaName}\") enter.");
            CompleteHeadersFromWebConfig();
            if(!Request.Headers.TryGetValues("x-hopex-task", out var hopexTask))
            {
                Logger.Info("Start job enter.");
                var startJobResult = ProcessQueryRequest(schemaName, args, version, data => CallAsyncMacroExecute(GraphQlMacro, data));
                Logger.Info("Start job leave.");
                Logger.Info($"GraphQlController.AsyncExecute(\"{schemaName}\") leave.");
                return startJobResult;
            }
            Logger.Info("Get job result enter.");
            var jobResult = CallAsyncMacroGetResult(hopexTask.FirstOrDefault());
            Logger.Info("Get job result leave.");
            Logger.Info($"GraphQlController.AsyncExecute(\"{schemaName}\") leave.");
            return jobResult;
        }

        private IHttpActionResult ProcessQueryRequest(string schemaName, InputArguments args, string version, Func<string, IHttpActionResult> callMacro)
        {
            args.WebServiceUrl = Request.RequestUri.AbsoluteUri.Substring(0, Request.RequestUri.AbsoluteUri.IndexOf("/api/", StringComparison.Ordinal));

            HopexContext hopexContext = null;
            if (Request.Headers.Contains("x-hopex-environment-id"))
            {
                hopexContext = new HopexContext
                {
                    EnvironmentId = Request.Headers.GetValues("x-hopex-environment-id").FirstOrDefault(),
                };
            }
            else if (!TryParseHopexContext(ref hopexContext))
            {
                const string message = "Parameters \"x-hopex-environment-id\", \"x-hopex-repository-id\", \"x-hopex-profile-id\" and optionally \"x-hopex-language-data-id\" and \"x-hopex-language-gui-id\" must be set in the headers of your request.";
                Logger.Debug(message);
                return BadRequest(message);
            }
            var headers = new Dictionary<string, string []>
            {
                {
                    "EnvironmentId", new[] {hopexContext.EnvironmentId}
                }
            };
            var path = BuildMacroPath("graphql", schemaName, version);
            var data = new CallMacroArguments<object>(headers, path, args);
            return callMacro(data.ToString());
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
            Logger.Info($"GraphQlController.ExportSchema(\"{schemaName}\", \"{version}\") enter.");
            HopexContext hopexContext = null;
            if (Request.Headers.Contains("x-hopex-environment-id"))
            {
                hopexContext = new HopexContext
                {
                    EnvironmentId = Request.Headers.GetValues("x-hopex-environment-id").FirstOrDefault(),
                };
            }
            else if (!TryParseHopexContext(ref hopexContext))
            {
                const string message = "Parameters \"x-hopex-environment-id\", \"x-hopex-repository-id\", \"x-hopex-profile-id\" and optionally \"x-hopex-language-data-id\" and \"x-hopex-language-gui-id\" must be set in the headers of your request.";
                Logger.Debug(message);
                return BadRequest(message);
            }
            var headers = new Dictionary<string, string []>
            {
                {
                    "EnvironmentId", new[] {hopexContext.EnvironmentId}
                }
            };
            var path = BuildMacroPath("schema", schemaName, version);
            var data = new CallMacroArguments<object>(headers, path, null);

            var macroResult = CallMacro(GraphQlMacro, data.ToString());

            var result = ProcessMacroResult(macroResult, () =>
            {
                var response = JsonConvert.DeserializeObject<SchemaMacroResponse>(macroResult.Content);
                var bytes = Encoding.UTF8.GetBytes(response.Schema);
                var contentStream = new StreamContent(new MemoryStream(bytes));
                var actionResult = new HttpResponseMessage(HttpStatusCode.OK) { Content = contentStream };
                actionResult.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                actionResult.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = $"{schemaName}.graphql" };
                return ResponseMessage(actionResult);
            });

            Logger.Info($"GraphQlController.ExportSchema(\"{schemaName}\", \"{version}\") leave.");
            return result;
        }

        protected virtual bool TryParseHopexContext(ref HopexContext hopexContext)
        {
            return Request.Headers.TryGetValues("x-hopex-context", out var hopexContextHeader)
                   && HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out hopexContext);
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

        protected override IHttpActionResult BuildActionResultFrom(AsyncMacroResult macroResult)
        {
            var response = JsonConvert.DeserializeObject<GraphQlResponse>(macroResult.Result);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return Content(response.HttpStatusCode, new { response.Error });
            }
            return Ok(JsonConvert.DeserializeObject(response.Result));
        }
    }
}
