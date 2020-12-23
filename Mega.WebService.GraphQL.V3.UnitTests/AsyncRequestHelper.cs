using Mega.WebService.GraphQL.Controllers;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    class AsyncRequestHelper
    {
        internal static IHttpActionResult PlayAsyncRequest(BaseController controller, Func<IHttpActionResult> controllerAsyncCall)
        {
            var actionResult = controllerAsyncCall();

            var content = actionResult as ResponseMessageResult;
            var taskId = content.Response.Headers.GetValues("x-hopex-task").First();
            controller.Request.Headers.Add("x-hopex-task", taskId);

            actionResult = controllerAsyncCall();
            return actionResult;
        }
    }
}
