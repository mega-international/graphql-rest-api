using FluentAssertions;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.Utils;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using Moq;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Xunit;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class BaseController_should
    {
        readonly TestableBaseController controller;
        readonly HttpRequestMessage request = new HttpRequestMessage();
        readonly Mock<IHopexServiceFinder> mockHopexServiceFinder = new Mock<IHopexServiceFinder>();
        readonly Mock<IHopexService> mockHopexService = new Mock<IHopexService>();
        readonly AsyncMacroResult inProgress = new AsyncMacroResult { Status = "InProgress", ActionId = "myActionId" };
        readonly AsyncMacroResult terminated = new AsyncMacroResult { Status = "Terminate", ActionId = "myActionId", Result = "async macro result" };

        public BaseController_should()
        {
            request.Properties.Add("UserInfo", new UserInfo());

            mockHopexServiceFinder.Setup(s => s.GetService("https://mymwas", "mySecureKey")).Returns(mockHopexService.Object).Verifiable();
            mockHopexServiceFinder.Setup(s => s.GetMwasService("https://mymwas", "mySessionToken")).Returns(mockHopexService.Object);

            mockHopexService.Setup(s => s.CallMacro("AAC8AB1E5D25678E", "", It.IsAny<GenerationContext>(), null)).Returns("macro result");
            mockHopexService.Setup(s => s.HopexSessionToken).Returns("mySessionToken");
            mockHopexService.Setup(s => s.MwasUrl).Returns("https://mymwas");

            controller = new TestableBaseController(mockHopexServiceFinder.Object) { Request = request };
        }   

        [Fact]
        public void Fail_macro_call_when_no_hopex_context_given()
        {
            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E");

            actual.ErrorType.Should().Be("BadRequest");
            actual.Content.Should().Contain("x-hopex-environment-id");
        }

        [Fact]
        public void Fail_macro_call_when_no_mwas_available()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            
            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E");

            actual.ErrorType.Should().Be("BadRequest");
            actual.Content.Should().Contain("MWAS");
            mockHopexServiceFinder.Verify(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true));
        }

        [Fact]
        public void Fail_macro_call_when_session_cannot_be_opened()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            
            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E");

            actual.ErrorType.Should().Be("BadRequest");
            actual.Content.Should().Contain("session");
            mockHopexServiceFinder.Verify();
        }

        [Fact]
        public void Call_a_macro_synchronously()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            mockHopexService.Setup(s => s.TryOpenSession(It.IsAny<MwasSettings>(), It.IsAny<MwasSessionConnectionParameters>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), true, true))
                .Returns(true);
            
            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E");

            actual.ErrorType.Should().Be("None");
            actual.Content.Should().Contain("macro result");
        }

        [Fact]
        public void Call_a_macro_synchronously_using_hopex_context_in_web_config()
        {
            PutHopexContextInWebConfig();
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            mockHopexService.Setup(s => s.TryOpenSession(It.IsAny<MwasSettings>(), It.IsAny<MwasSessionConnectionParameters>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), true, true))
                .Returns(true);

            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E");

            actual.ErrorType.Should().Be("None");
            actual.Content.Should().Contain("macro result");
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void Be_able_to_close_the_session(bool closeSession, int expectedCalls)
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            mockHopexService.Setup(s => s.TryOpenSession(It.IsAny<MwasSettings>(), It.IsAny<MwasSessionConnectionParameters>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), !closeSession, true))
                .Returns(true);

            var actual = controller.PublicCallMacro("AAC8AB1E5D25678E", closeSession: closeSession);

            mockHopexService.Verify(s => s.CloseUpdateSession(), Times.Exactly(expectedCalls));
        }

        [Fact]
        public void Fail_async_macro_call_when_no_hopex_context_given()
        {
            var actual = controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");

            actual.Should().BeBadRequest("*x-hopex-environment-id*");
        }

        [Fact]
        public void Fail_async_macro_call_when_no_mwas_available()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");

            var actual = controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");

            actual.Should().BeBadRequest("*MWAS*");
            mockHopexServiceFinder.Verify(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true));
        }

        [Fact]
        public void Fail_async_macro_call_when_session_cannot_be_opened()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");

            var actual = controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");

            actual.Should().BeBadRequest("*session*");
            mockHopexServiceFinder.Verify();
        }

        [Fact]
        public void Result_code_408_timeout_when_find_session_ends_with_taskcanceledexception()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Throws(new TaskCanceledException());

            var actual = controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");

            actual.Should().BeError(HttpStatusCode.RequestTimeout, "Timeout of 100 seconds reached while waiting for FindSession request.") ;
        }

        [Fact]
        public void Result_code_408_timeout_when_try_open_session_ends_with_taskcanceledexception()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            mockHopexService.Setup(s => s.TryOpenSession(It.IsAny<MwasSettings>(), It.IsAny<MwasSessionConnectionParameters>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), true))
                .Throws(new TaskCanceledException());

            var actual = controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");

            actual.Should().BeError(HttpStatusCode.RequestTimeout, "Timeout of 100 seconds reached while waiting for TryOpenSession request.");
        }

        [Fact]
        public void Call_a_macro_asynchronously()
        {
            var actual = LaunchInProgressMacro();

            actual.Response.Headers.GetValues("x-hopex-task").First().Should().Be("myActionId");
        }
        

        [Fact]
        public void Call_a_macro_asynchronously_using_hopex_context_in_web_config()
        {
            PutHopexContextInWebConfig();
            
            var actual = LaunchInProgressMacroWithoutContextHeader();

            actual.Response.Headers.GetValues("x-hopex-task").First().Should().Be("myActionId");
        }        

        [Fact]
        public void Fail_to_retrieve_async_macro_result_if_session_token_not_forwarded()
        {
            LaunchInProgressMacro();

            var actual = controller.PublicCallAsyncMacroGetResult("myActionId", false);

            actual.Should().BeBadRequest("*token*");
        }


        [Fact]
        public void Treat_a_still_in_progress_async_macro_result()
        {
            var partialResult = LaunchInProgressMacro();
            request.Headers.Add("x-hopex-sessiontoken", partialResult.Response.Headers.GetValues("x-hopex-sessiontoken").First());
            mockHopexService.Setup(s => s.CallAsyncMacroGetResult("myActionId", It.IsAny<GenerationContext>(), null)).Returns(inProgress);

            var actual = (ResponseMessageResult) controller.PublicCallAsyncMacroGetResult("myActionId", false);

            actual.Response.Headers.GetValues("x-hopex-task").First().Should().Be("myActionId");
        }

        [Fact]
        public void Treat_a_terminated_async_macro_result()
        {
            var partialResult = LaunchInProgressMacro();
            request.Headers.Add("x-hopex-sessiontoken", partialResult.Response.Headers.GetValues("x-hopex-sessiontoken").First());
            mockHopexService.Setup(s => s.CallAsyncMacroGetResult("myActionId", It.IsAny<GenerationContext>(), null)).Returns(terminated);

            var actual = (OkNegotiatedContentResult<string>)controller.PublicCallAsyncMacroGetResult("myActionId", false);

            actual.Content.Should().Be("async macro result");
        }

        [Fact]
        public void Poll_multiple_times_for_slow_async_macro()
        {
            var partialResult = LaunchInProgressMacro();
            request.Headers.Add("x-hopex-sessiontoken", partialResult.Response.Headers.GetValues("x-hopex-sessiontoken").First());
            request.Headers.Add("x-hopex-wait", "10");
            mockHopexService.Setup(s => s.CallAsyncMacroGetResult("myActionId", It.IsAny<GenerationContext>(), null)).Returns(inProgress);

            var actual = (ResponseMessageResult)controller.PublicCallAsyncMacroGetResult("myActionId", false);

            mockHopexService.Verify(s => s.CallAsyncMacroGetResult("myActionId", It.IsAny<GenerationContext>(), null), Times.AtLeast(2));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public void Report_macro_error_bad_request_response(HttpStatusCode httpStatusCode)
        {
            var actual = controller.PublicProcessMacroResult(MacroResultBuilder.MacroError(httpStatusCode, "message"), null);

            actual.Should().BeError(httpStatusCode, "message");
        }

        private void PutHopexContextInWebConfig()
        {
            var appSettings = controller._fakeConfigurationManager.appSettings;
            appSettings.Add("EnvironmentId", "myEnv");
            appSettings.Add("RepositoryId", "myDb");
            appSettings.Add("ProfileId", "myProf");
        }

        private ResponseMessageResult LaunchInProgressMacro()
        {
            request.Headers.Add("x-hopex-context", @"{""EnvironmentId"":""myEnv"",""RepositoryId"":""myDb"",""ProfileId"":""myProf""}");
            return LaunchInProgressMacroWithoutContextHeader();
        }

        private ResponseMessageResult LaunchInProgressMacroWithoutContextHeader()
        {
            mockHopexServiceFinder.Setup(s => s.FindSession("https://myssp", "mySecureKey", "myEnv", null, null, "myProf", null, "RW", true)).Returns("https://mymwas");
            mockHopexService.Setup(s => s.TryOpenSession(It.IsAny<MwasSettings>(), It.IsAny<MwasSessionConnectionParameters>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), true, true))
                .Returns(true);
            mockHopexService.Setup(s => s.CallAsyncMacroExecute("AAC8AB1E5D25678E", "", It.IsAny<GenerationContext>(), null)).Returns(inProgress);

            return (ResponseMessageResult)controller.PublicCallAsyncMacroExecute("AAC8AB1E5D25678E");
        }
    }

   
    class TestableBaseController : BaseController
    {
        protected override int WaitStepMilliseconds => 5;
        internal readonly FakeConfigurationManager _fakeConfigurationManager;
        
        internal TestableBaseController(IHopexServiceFinder hopexServiceFinder) :
            base(new FakeConfigurationManager(), hopexServiceFinder)
        {
            _fakeConfigurationManager = (FakeConfigurationManager)_configurationManager;
        }

        internal WebServiceResult PublicCallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            return CallMacro(macroId, data, sessionMode, accessMode, closeSession);
        }

        internal IHttpActionResult PublicCallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            return CallAsyncMacroExecute(macroId, data, sessionMode, accessMode, closeSession);
        }

        internal IHttpActionResult PublicCallAsyncMacroGetResult(string hopexTask, bool closeSession = false)
        {
            return CallAsyncMacroGetResult(hopexTask, closeSession);
        }

        internal IHttpActionResult PublicProcessMacroResult(WebServiceResult result, Func<IHttpActionResult> process)
        {
            return ProcessMacroResult(result, process);
        }

        protected override IHttpActionResult BuildActionResultFrom(AsyncMacroResult macroResult)
        {
            return Ok(macroResult.Result);
        }
    }

    class FakeConfigurationManager : IConfigurationManager
    {
        internal NameValueCollection appSettings = new NameValueCollection();

        internal FakeConfigurationManager()
        {
            appSettings.Add("MegaSiteProvider", "https://myssp");
        }

        public NameValueCollection AppSettings => appSettings;

        public object GetSection(string sectionName)
        {
            if (sectionName == "secureAppSettings")
            {
                var col = new NameValueCollection();
                col.Add("SecurityKey", "mySecureKey");
                return col;
            }
            throw new NotImplementedException();
        }
    }
}
