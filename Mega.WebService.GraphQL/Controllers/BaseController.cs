using System;
using Mega.WebService.GraphQL.Models;
using log4net;
using Mega.Bridge.Filters;
using Mega.Bridge.Models;
using Mega.Bridge.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Configuration;
using System.Web.Http;
using Hopex.Common.JsonMessages;

namespace Mega.WebService.GraphQL.Controllers
{
    [HopexAuthenticationFilter]
    public class BaseController : ApiController
    {
        protected const string GraphQlMacro = "AAC8AB1E5D25678E";
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(BaseController));

        protected virtual WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            // Get UserInfo
            var userInfo = (UserInfo)Request.Properties ["UserInfo"];

            // Get values from x-hopex-context
            IEnumerable<string> hopexContextHeader;
            HopexContext hopexContext;
            if(!Request.Headers.TryGetValues("x-hopex-context", out hopexContextHeader) || !HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out hopexContext))
            {
                const string message = "Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}";
                Logger.Debug(message);
                return new WebServiceResult { ErrorType = "BadRequest", Content = message };
            }

            // Find the Hopex session
            var sspUrl = ConfigurationManager.AppSettings ["MegaSiteProvider"];
            var securityKey = ((NameValueCollection)WebConfigurationManager.GetSection("secureAppSettings")) ["SecurityKey"];
            var mwasUrl = HopexService.FindSession(sspUrl, securityKey, hopexContext.EnvironmentId, hopexContext.DataLanguageId, hopexContext.GuiLanguageId, hopexContext.ProfileId, userInfo.HopexAuthPerson, true);
            if(mwasUrl == null)
            {
                const string message = "Unable to get MWAS url. Please retry later and check your configuration if it doesn't work.";
                Logger.Debug(message);
                return new WebServiceResult { ErrorType = "BadRequest", Content = message };
            }
            mwasUrl = mwasUrl.ToLower().Replace("hopexmwas", "hopexapimwas");

            // Open the Hopex session
            var hopexService = new HopexService(mwasUrl, securityKey);
            var mwasSettings = InitMwasSettings();
            var mwasSessionConnectionParameters = InitMwasSessionConnectionParameters(sessionMode, accessMode, hopexContext, userInfo);
            if(!hopexService.TryOpenSession(mwasSettings, mwasSessionConnectionParameters, findSession: !closeSession))
            {
                var message = "Unable to open an Hopex session. Please check the values in the HopexContext header and retry.";
                Logger.Debug(message);
                return new WebServiceResult { ErrorType = "BadRequest", Content = message };
            }

            // Call the macro
            string macroResult = "";
            try
            {
                macroResult = hopexService.CallMacro(macroId, data);
            }
            catch(Exception)
            {

            }

            // Close the Hopex session
            if(closeSession)
            {
                hopexService.CloseUpdateSession();
            }

            // Return the macro result
            return new WebServiceResult { ErrorType = "None", Content = macroResult };
        }

        protected IHttpActionResult CallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false, TimeSpan? wait = null, int waitStepMilliseconds = 100)
        {
            if(wait == null)
            {
                wait = TimeSpan.Zero;
            }

            // Get UserInfo
            var userInfo = (UserInfo)Request.Properties ["UserInfo"];

            // Get values from x-hopex-context
            if(!Request.Headers.TryGetValues("x-hopex-context", out var hopexContextHeader) || !HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out var hopexContext))
            {
                const string message = "Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}";
                Logger.Debug(message);
                return BadRequest(message);
            }

            // Find the Hopex session
            var sspUrl = ConfigurationManager.AppSettings ["MegaSiteProvider"];
            var securityKey = ((NameValueCollection)WebConfigurationManager.GetSection("secureAppSettings")) ["SecurityKey"];
            var mwasUrl = HopexService.FindSession(sspUrl, securityKey, hopexContext.EnvironmentId, hopexContext.DataLanguageId, hopexContext.GuiLanguageId, hopexContext.ProfileId, userInfo.HopexAuthPerson, true);
            if(mwasUrl == null)
            {
                const string message = "Unable to get MWAS url. Please retry later and check your configuration if it doesn't work.";
                Logger.Debug(message);
                return BadRequest(message);
            }
            mwasUrl = mwasUrl.ToLower().Replace("hopexmwas", "hopexapimwas");

            // Open the Hopex session
            var hopexService = new HopexService(mwasUrl, securityKey);
            var mwasSettings = InitMwasSettings();
            var mwasSessionConnectionParameters = InitMwasSessionConnectionParameters(sessionMode, accessMode, hopexContext, userInfo);
            if(!hopexService.TryOpenSession(mwasSettings, mwasSessionConnectionParameters, findSession: true))
            {
                var message = "Unable to open an Hopex session. Please check the values in the HopexContext header and retry.";
                Logger.Debug(message);
                return BadRequest(message);
            }

            // Call the execution of the macro in async mode
            var asyncMacroResult = hopexService.CallAsyncMacroExecute(macroId, data);

            // If error occurs
            if(asyncMacroResult.Status != "InProgress")
            {
                if(closeSession)
                {
                    hopexService.CloseSession();
                }
                return Ok(asyncMacroResult);
            }

            // If wait time is zero
            if(wait <= TimeSpan.Zero)
            {
                var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
                var hopexSession = HopexServiceHelper.EncryptHopexSessionInfo(hopexService.MwasUrl, hopexService.HopexSessionToken);
                response.Headers.Add("x-hopex-sessiontoken", hopexSession);
                response.Headers.Add("x-hopex-task", asyncMacroResult.ActionId);
                return ResponseMessage(response);
            }

            // Get result
            return WaitForResult(hopexService, asyncMacroResult.ActionId, closeSession, wait.Value, waitStepMilliseconds);
        }

        protected IHttpActionResult CallAsyncMacroGetResult(string hopexTask, bool closeSession = true, TimeSpan? wait = null, int waitStepMilliseconds = 100)
        {
            if(wait == null)
            {
                wait = TimeSpan.Zero;
            }

            // Get values from x-hopex-sessiontoken
            if(!Request.Headers.TryGetValues("x-hopex-sessiontoken", out var hopexSessionHeader) || !HopexServiceHelper.TryGetHopexSessionInfo(hopexSessionHeader.FirstOrDefault(), out var hopexSessionInfo))
            {
                const string message = "Parameter \"x-hopex-sessiontoken\" must be set in the header of your request.";
                Logger.Debug(message);
                return BadRequest(message);
            }

            // Get the existing Hopex session 
            var hopexService = HopexServiceHelper.GetMwasService(hopexSessionInfo.MwasUrl, hopexSessionInfo.HopexSessionToken);

            // Get result
            return WaitForResult(hopexService, hopexTask, closeSession, wait.Value, waitStepMilliseconds);
        }

        private IHttpActionResult WaitForResult(HopexService hopexService, string hopexTask, bool closeSession, TimeSpan wait, int waitStepMilliseconds)
        {
            do
            {
                // Call the execution result of the macro in async mode
                var asyncMacroResult = hopexService.CallAsyncMacroGetResult(hopexTask);

                // Return status if action is not finished and wait time is over
                if(asyncMacroResult.Status == "InProgress")
                {
                    if(wait <= TimeSpan.Zero)
                    {
                        if(closeSession)
                        {
                            hopexService.CloseUpdateSession();
                        }

                        var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
                        var hopexSession = HopexServiceHelper.EncryptHopexSessionInfo(hopexService.MwasUrl, hopexService.HopexSessionToken);
                        response.Headers.Add("x-hopex-sessiontoken", hopexSession);
                        response.Headers.Add("x-hopex-task", asyncMacroResult.ActionId);
                        return ResponseMessage(response);
                    }
                }
                // Else return result
                else
                {
                    if(closeSession)
                    {
                        hopexService.CloseUpdateSession();
                    }

                    switch (asyncMacroResult.Status)
                    {
                        case "Terminate":
                        {
                            var response = JsonConvert.DeserializeObject<GraphQlResponse>(asyncMacroResult.Result);
                            if(response.HttpStatusCode != HttpStatusCode.OK)
                            {
                                return Content(response.HttpStatusCode, new { response.Error });
                            }
                            return Ok(JsonConvert.DeserializeObject(response.Result));
                        }
                            //return Ok(JsonConvert.DeserializeObject(asyncMacroResult.Result));
                        case "UnknownActionId":
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, asyncMacroResult));
                        default:
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, asyncMacroResult));
                    }
                }

                // Wait and decrement wait time
                Thread.Sleep(waitStepMilliseconds);
                wait = wait.Add(TimeSpan.FromMilliseconds(-waitStepMilliseconds));

            } while(true);
        }

        private static MwasSettings InitMwasSettings()
        {
            var mwasSettings = new MwasSettings
            {
                CacheFileRootPath = ConfigurationManager.AppSettings ["CacheFileRootPath"],
                MaxMegaSessionCount = ConfigurationManager.AppSettings ["MaxMegaSessionCount"],
                OpenSessionTimeout = ConfigurationManager.AppSettings ["OpenSessionTimeout"],
                StatMinPanelTime = ConfigurationManager.AppSettings ["StatMinPanelTime"],
                MultiThreadLimit = ConfigurationManager.AppSettings ["MultiThreadLimit"],
                MinConnectionDuration = ConfigurationManager.AppSettings ["MinConnectionDuration"],
                MaxConnectionRetry = ConfigurationManager.AppSettings ["MaxConnectionRetry"],
                CacheSerialize = ConfigurationManager.AppSettings ["CacheSerialize"],
                CacheFileDiscard = ConfigurationManager.AppSettings ["CacheFileDiscard"],
                CheckState = ConfigurationManager.AppSettings ["CheckState"],
                LazyLog = ConfigurationManager.AppSettings ["LazyLog"],
                DisableCache = ConfigurationManager.AppSettings ["DisableCache"],
                AllowAnonymousConnection = ConfigurationManager.AppSettings ["AllowAnonymousConnection"],
                LogRequest = ConfigurationManager.AppSettings ["LogRequest"]
            };
            return mwasSettings;
        }

        private MwasSessionConnectionParameters InitMwasSessionConnectionParameters(string sessionMode, string accessMode, HopexContext hopexContext, UserInfo userInfo)
        {
            var mwasSessionConnectionParameters = new MwasSessionConnectionParameters
            {
                Environment = hopexContext.EnvironmentId,
                Database = hopexContext.RepositoryId,
                Profile = hopexContext.ProfileId,
                DataLanguage = hopexContext.DataLanguageId,
                GuiLanguage = hopexContext.GuiLanguageId,
                Person = userInfo.HopexAuthPerson,
                Login = userInfo.HopexAuthLogin,
                AuthenticationToken = userInfo.HopexAuthToken,
                SessionMode = sessionMode,
                AccessMode = accessMode
            };
            return mwasSessionConnectionParameters;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        protected IHttpActionResult ProcessMacroResult(WebServiceResult result, Func<IHttpActionResult> process)
        {
            if (result.ErrorType == "None")
            {
                try
                {
                    if (TryParseErrorMacroResponse(result, out ErrorMacroResponse errorMacroResponse))
                        return Content(errorMacroResponse.HttpStatusCode, new ErrorContent(errorMacroResponse.Error));
                    return process();
                }
                catch
                {
                    return InternalServerError(new Exception(result.Content));
                }
            }

            return FormatResult(result);
        }

        private bool TryParseErrorMacroResponse(WebServiceResult result, out ErrorMacroResponse errorMacroResponse)
        {
            errorMacroResponse = null;
            if (result.Content == null) return false;
            bool isErrorMacroResponse = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { isErrorMacroResponse = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            errorMacroResponse = JsonConvert.DeserializeObject<ErrorMacroResponse>(result.Content, settings);
            return isErrorMacroResponse;
        }

        protected IHttpActionResult FormatResult(WebServiceResult result)
        {
            switch (result.ErrorType)
            {
                case "None":
                    return Ok(result.Content);
                case "BadRequest":
                    return BadRequest(result.Content);
                default:
                    return InternalServerError(new Exception($"{result.ErrorType}: {result.Content}"));
            }
        }
    }
}
