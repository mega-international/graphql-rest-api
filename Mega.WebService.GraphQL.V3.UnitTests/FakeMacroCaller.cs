using System;
using FluentAssertions;
using Mega.Bridge.Models;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using System.Net.Http;
using System.Net;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    class FakeMacroCaller
    {
        private IMacroCall _macroCall;
        
        internal FakeMacroCaller(IMacroCall macroCaller)
        {
            _macroCall = macroCaller;
        }

        public virtual WebServiceResult CallMacro(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            return _macroCall.CallMacro(macroId, data);
        }

        private string _taskId = new Random().Next().ToString();
        private WebServiceResult _lastResult;

        public virtual HttpResponseMessage CallAsyncMacroExecute(string macroId, string data = "", string sessionMode = "MS", string accessMode = "RW", bool closeSession = false)
        {
            _lastResult = _macroCall.CallMacro(macroId, data);
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
            response.Headers.Add("x-hopex-task", _taskId);
            return response;
        }

        public virtual AsyncMacroResult CallAsyncMacroGetResult(string hopexTask, bool closeSession = false)
        {
            hopexTask.Should().Be(_taskId);
            return new AsyncMacroResult { Result = _lastResult.Content };
        }
    }
}
