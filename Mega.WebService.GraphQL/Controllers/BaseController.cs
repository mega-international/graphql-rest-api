using Hopex.Common.JsonMessages;
using log4net;
using Mega.Bridge.Filters;
using Mega.Bridge.Models;
using Mega.Bridge.Services;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [HopexAuthenticationFilter]
    public class BaseController : ApiController
    {
        protected const string GraphQlMacro = "AAC8AB1E5D25678E";
        protected static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected virtual int WaitStepMilliseconds => 100;

        private readonly IConfigurationManager _configurationManager;
        private readonly IHopexServiceFinder _hopexServiceFinder;

        public BaseController()
        {
            _configurationManager = new RealConfigurationManager();
            _hopexServiceFinder = new RealHopexServiceFinder();
        }

        public BaseController(IConfigurationManager configurationManager, IHopexServiceFinder hopexServiceFinder)
        {
            _configurationManager = configurationManager;
            _hopexServiceFinder = hopexServiceFinder;
        }
        
        protected virtual WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            if (!TryGetHopexService(sessionMode, accessMode, closeSession, out var hopexService, out var error))
            {
               return error;
            }

            // Call the macro
            var macroResult = "";
            try
            {
                macroResult = hopexService.CallMacro(macroId, data);
            }
            catch
            {
                // ignored
            }

            // Close the Hopex session
            if(closeSession)
            {
                hopexService.CloseUpdateSession();
            }

            // Return the macro result
            return new WebServiceResult { ErrorType = "None", Content = macroResult };
        }

        private bool ExecuteTimedOut<T>(string funcName, Func<T> function, out T result, out WebServiceResult error)
        {
            result = default(T);
            error = null;
            try
            {
                result = function();
                return true;
            }
            catch (TaskCanceledException)
            {
                var message = $"Timeout of 100 seconds reached while waiting for {funcName} request.";
                Logger.Debug(message);
                error = new WebServiceResult { ErrorType = "Timeout", Content = message };
                return false;
            }
        }

        private bool TryGetHopexService(string sessionMode, string accessMode, bool closeSession, out IHopexService hopexService, out WebServiceResult error)
        {
            hopexService = null;
            error = null;

            // Get UserInfo
            var userInfo = (UserInfo)Request.Properties["UserInfo"];

            // Get values from x-hopex-context
            IEnumerable<string> hopexContextHeader;
            HopexContext hopexContext;
            if (!Request.Headers.TryGetValues("x-hopex-context", out hopexContextHeader) || !HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out hopexContext))
            {
                const string message = "Parameter \"x-hopex-context\" must be set in the header of your request. Example: HopexContext:{\"EnvironmentId\":\"IdAbs\",\"RepositoryId\":\"IdAbs\",\"ProfileId\":\"IdAbs\",\"DataLanguageId\":\"IdAbs\",,\"GuiLanguageId\":\"IdAbs\"}";
                Logger.Debug(message);
                error = new WebServiceResult { ErrorType = "BadRequest", Content = message };
                return false;
            }

            // Find the Hopex session
            var sspUrl = _configurationManager.AppSettings["MegaSiteProvider"];
            var securityKey = ((NameValueCollection)_configurationManager.GetSection("secureAppSettings"))?["SecurityKey"];

            Request.Headers.TryGetValues("x-session-type", out var hopexSessionTypeHeader);
            Enum.TryParse(hopexSessionTypeHeader?.FirstOrDefault(), out HopexSessionType hopeSessionType);

            string FuncFindSession() => _hopexServiceFinder.FindSession(sspUrl, securityKey, hopexContext.EnvironmentId, hopexContext.DataLanguageId, hopexContext.GuiLanguageId, hopexContext.ProfileId, userInfo.HopexAuthPerson, hopeSessionType == HopexSessionType.API);
            if (!ExecuteTimedOut("FindSession", FuncFindSession, out string mwasUrl, out error))
            {
                return false;
            }
            if (mwasUrl == null)
            {
                const string message = "Unable to get MWAS url. Please retry later and check your configuration if it doesn't work.";
                Logger.Debug(message);
                error = new WebServiceResult { ErrorType = "BadRequest", Content = message };
                return false;
            }
            mwasUrl = mwasUrl.ToLower().Replace("hopexmwas", "hopexapimwas");

            // Open the Hopex session
            hopexService = _hopexServiceFinder.GetService(mwasUrl, securityKey);
            var mwasSettings = InitMwasSettings();
            var mwasSessionConnectionParameters = InitMwasSessionConnectionParameters(sessionMode, accessMode, hopexContext, userInfo);

            var hopexServiceCopy = hopexService;
            bool FuncTryOpenSession() => hopexServiceCopy.TryOpenSession(mwasSettings, mwasSessionConnectionParameters, findSession: !closeSession);
            bool sessionOpened;
            if (!ExecuteTimedOut("TryOpenSession", FuncTryOpenSession, out sessionOpened, out error))
            {
                return false;
            }
            if (!sessionOpened)
            {
                var message = "Unable to open an Hopex session. Please check the values in the HopexContext header and retry.";
                Logger.Debug(message);
                error = new WebServiceResult { ErrorType = "BadRequest", Content = message };
                return false;
            }
            return true;
        }

        protected virtual IHttpActionResult CallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            if (!TryGetHopexService(sessionMode, accessMode, closeSession, out var hopexService, out var error))
                return FormatResult(error);

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

            var wait = ReadWaitHeader();
            // If wait time is zero
            if (wait <= TimeSpan.Zero)
                return BuildTaskInProgressActionResult(hopexService, asyncMacroResult);

            // Get result
            return WaitForResult(hopexService, asyncMacroResult.ActionId, closeSession);
        }

        protected virtual IHttpActionResult CallAsyncMacroGetResult(string hopexTask, bool closeSession = false)
        {
            // Get values from x-hopex-sessiontoken
            if(!Request.Headers.TryGetValues("x-hopex-sessiontoken", out var hopexSessionHeader) || !HopexServiceHelper.TryGetHopexSessionInfo(hopexSessionHeader.FirstOrDefault(), out var hopexSessionInfo))
            {
                const string message = "Parameter \"x-hopex-sessiontoken\" must be set in the header of your request.";
                Logger.Debug(message);
                return BadRequest(message);
            }

            // Get the existing Hopex session 
            var hopexService = _hopexServiceFinder.GetMwasService(hopexSessionInfo.MwasUrl, hopexSessionInfo.HopexSessionToken);

            // Get result
            return WaitForResult(hopexService, hopexTask, closeSession);
        }

        private IHttpActionResult WaitForResult(IHopexService hopexService, string hopexTask, bool closeSession)
        {
            var stopwatch = new Stopwatch();
            var wait = ReadWaitHeader();
            do
            {
                // Call the execution result of the macro in async mode
                stopwatch.Restart();
                Logger.Info("CallAsyncMacroGetResult");
                var asyncMacroResult = hopexService.CallAsyncMacroGetResult(hopexTask);
                Logger.Info("CallAsyncMacroGetResult ended: " + Math.Round(stopwatch.Elapsed.TotalMilliseconds) + " ms");
                wait = wait - stopwatch.Elapsed;

                // Return status if action is not finished and wait time is over
                if(asyncMacroResult.Status == "InProgress")
                {
                    if(wait <= TimeSpan.Zero)
                    {
                        if (closeSession)
                        {
                            hopexService.CloseUpdateSession();
                        }

                        return BuildTaskInProgressActionResult(hopexService, asyncMacroResult);
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
                            return BuildActionResultFrom(asyncMacroResult);
                        case "UnknownActionId":
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, asyncMacroResult));
                        default:
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, asyncMacroResult));
                    }
                }

                // Wait and decrement wait time
                Thread.Sleep(WaitStepMilliseconds);
                wait = wait.Add(TimeSpan.FromMilliseconds(-WaitStepMilliseconds));

            } while(true);
        }

        private TimeSpan ReadWaitHeader()
        {
            if (Request.Headers.TryGetValues("x-hopex-wait", out var hopexWait) && int.TryParse(hopexWait.FirstOrDefault(), out var hopexWaitMilliseconds))
                return TimeSpan.FromMilliseconds(hopexWaitMilliseconds);
            return TimeSpan.Zero;
        }

        private IHttpActionResult BuildTaskInProgressActionResult(IHopexService hopexService, AsyncMacroResult asyncMacroResult)
        {
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
            var hopexSession = HopexServiceHelper.EncryptHopexSessionInfo(hopexService.MwasUrl, hopexService.HopexSessionToken);
            response.Headers.Add("x-hopex-sessiontoken", hopexSession);
            response.Headers.Add("x-hopex-task", asyncMacroResult.ActionId);
            return ResponseMessage(response);
        }

        protected virtual IHttpActionResult BuildActionResultFrom(AsyncMacroResult asyncMacroResult)
        {
            throw new NotImplementedException();
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
                case "Timeout":
                    return Content(HttpStatusCode.RequestTimeout, new ErrorContent(result.Content));
                default:
                    return InternalServerError(new Exception($"{result.ErrorType}: {result.Content}"));
            }
        }
    }
}
