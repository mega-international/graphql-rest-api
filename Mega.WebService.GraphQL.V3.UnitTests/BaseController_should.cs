using Mega.WebService.GraphQL.Controllers;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class BaseController_should
    {
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public void Report_macro_error_bad_request_response(HttpStatusCode httpStatusCode)
        {
            var controller = new TestableBaseController();

            var actual = controller.PublicProcessMacroResult(MacroResultBuilder.MacroError(httpStatusCode, "message"), null);

            actual.Should().BeError(httpStatusCode, "message");
        }
    }

   
    class TestableBaseController : BaseController
    {
        internal IHttpActionResult PublicProcessMacroResult(WebServiceResult result, Func<IHttpActionResult> process)
        {
            return ProcessMacroResult(result, process);
        }
    }
}
