using Hopex.Common.JsonMessages;
using log4net;
using Mega.Bridge.Models;
using Mega.Bridge.Services;
using Mega.WebService.GraphQL.Filters;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.Utils;
using Newtonsoft.Json;
using System;
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

        protected readonly IConfigurationManager _configurationManager;
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
                macroResult = hopexService.CallMacro(macroId, data, new GenerationContext {GenerationMode = "anywhere"});
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
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
                Logger.Error(message);
                error = new WebServiceResult { ErrorType = "Timeout", Content = message };
                return false;
            }
        }

        private bool TryGetHopexService(string sessionMode, string accessMode, bool closeSession, out IHopexService hopexService, out WebServiceResult error)
        {
            Logger.Info("TryGetHopexService enter");

            hopexService = null;
            error = null;

            // Get UserInfo
            var userInfo = (UserInfo)Request.Properties["UserInfo"];

            // Get values from x-hopex-context
            CompleteHeadersFromWebConfig();
            HopexContext hopexContext;
            if (Request.Headers.Contains("x-hopex-environment-id") && Request.Headers.Contains("x-hopex-repository-id") && Request.Headers.Contains("x-hopex-profile-id"))
            {
                hopexContext = new HopexContext
                {
                    EnvironmentId = Request.Headers.GetValues("x-hopex-environment-id").FirstOrDefault(),
                    RepositoryId = Request.Headers.GetValues("x-hopex-repository-id").FirstOrDefault(),
                    ProfileId = Request.Headers.GetValues("x-hopex-profile-id").FirstOrDefault()
                };
                if (Request.Headers.Contains("x-hopex-language-data-id"))
                {
                    hopexContext.DataLanguageId = Request.Headers.GetValues("x-hopex-language-data-id").FirstOrDefault();
                }
                if (Request.Headers.Contains("x-hopex-language-gui-id"))
                {
                    hopexContext.GuiLanguageId = Request.Headers.GetValues("x-hopex-language-gui-id").FirstOrDefault();
                }
            }
            else if (!Request.Headers.TryGetValues("x-hopex-context", out var hopexContextHeader) || !HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out hopexContext))
            {
                const string message = "Parameters \"x-hopex-environment-id\", \"x-hopex-repository-id\", \"x-hopex-profile-id\" and optionally \"x-hopex-language-data-id\" and \"x-hopex-language-gui-id\" must be set in the headers of your request.";
                Logger.Debug(message);
                error = new WebServiceResult { ErrorType = "BadRequest", Content = message };
                return false;
            }

            // Find the Hopex session
            var sspUrl = _configurationManager.AppSettings["MegaSiteProvider"];
            var securityKey = ((NameValueCollection)_configurationManager.GetSection("secureAppSettings"))?["SecurityKey"];

            Request.Headers.TryGetValues("x-hopex-session-type", out var hopexSessionTypeHeader);
            Enum.TryParse(hopexSessionTypeHeader?.FirstOrDefault(), out HopexSessionType hopexSessionType);

            string FuncFindSession() => _hopexServiceFinder.FindSession(sspUrl, securityKey, hopexContext.EnvironmentId, hopexContext.DataLanguageId, hopexContext.GuiLanguageId, hopexContext.ProfileId, userInfo.HopexAuthPerson, accessMode, hopexSessionType == HopexSessionType.API);
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
            if (hopexSessionType == HopexSessionType.API)
            {
                mwasUrl = mwasUrl.ToLower().Replace("hopexmwas", "hopexapimwas");
            }

            // Open the Hopex session
            hopexService = _hopexServiceFinder.GetService(mwasUrl, securityKey);
            var mwasSettings = InitMwasSettings();
            var mwasSessionConnectionParameters = InitMwasSessionConnectionParameters(sessionMode, accessMode, hopexContext, userInfo);

            var hopexServiceCopy = hopexService;
            try
            {
                bool FuncTryOpenSession() => hopexServiceCopy.TryOpenSession(mwasSettings, mwasSessionConnectionParameters, findSession: !closeSession, useHopexApiMwas: hopexSessionType == HopexSessionType.API);
                if (!ExecuteTimedOut("TryOpenSession", FuncTryOpenSession, out var sessionOpened, out error))
                {
                    return false;
                }
                if (!sessionOpened)
                {
                    throw new Exception("Unable to open an Hopex session.");
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message + "Check your headers parameters, the security key in web.config and see the MWAS log.");
                error = new WebServiceResult { ErrorType = "BadRequest", Content = ex.Message };
                return false;
            }

            Logger.Info("TryGetHopexService leave: OK");
            return true;
        }

        protected virtual IHttpActionResult CallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            if (!TryGetHopexService(sessionMode, accessMode, closeSession, out var hopexService, out var error))
                return FormatResult(error);

            // Call the execution of the macro in async mode
            var asyncMacroResult = hopexService.CallAsyncMacroExecute(macroId, data, new GenerationContext {GenerationMode = "anywhere"});

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
                var asyncMacroResult = hopexService.CallAsyncMacroGetResult(hopexTask, new GenerationContext {GenerationMode = "anywhere"});
                Logger.Info("CallAsyncMacroGetResult ended: " + stopwatch.ElapsedMilliseconds + " ms");
                wait -= stopwatch.Elapsed;

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

        protected IHttpActionResult ProcessMacroResult(WebServiceResult result, Func<IHttpActionResult> process)
        {
            if (result.ErrorType == "None")
            {
                try
                {
                    if (TryParseErrorMacroResponse(result, out ErrorMacroResponse errorMacroResponse))
                    {
                        return Content(errorMacroResponse.HttpStatusCode, new ErrorContent(errorMacroResponse.Error));
                    }
                    return process();
                }
                catch (Exception ex)
                {
                    Logger.Error(result.Content, ex);
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

        protected void CompleteHeadersFromWebConfig()
        {
            if (!Request.Headers.Contains("x-hopex-context"))
            {
                AddMissingHeaderFromWebConfig("x-hopex-environment-id", "EnvironmentId");
                AddMissingHeaderFromWebConfig("x-hopex-repository-id", "RepositoryId");
                AddMissingHeaderFromWebConfig("x-hopex-profile-id", "ProfileId");                
            }
        }

        private void AddMissingHeaderFromWebConfig(string header, string setting)
        {
            if (!Request.Headers.Contains(header) && _configurationManager.AppSettings[setting] != null)
                Request.Headers.Add(header, _configurationManager.AppSettings[setting]);
        }
    }
}
