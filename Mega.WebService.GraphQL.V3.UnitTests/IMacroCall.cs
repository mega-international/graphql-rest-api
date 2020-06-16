using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json;
using System.Net;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public interface IMacroCall
    {
        WebServiceResult CallMacro(string macroId, string data);
    }

    public static class MacroResultBuilder {
        public static WebServiceResult Ok(string content)
        {
            return new WebServiceResult
            {
                ErrorType = "None",
                Content = content
            };
        }

        public static WebServiceResult MacroError(HttpStatusCode httpStatusCode, string message)
        {
            return new WebServiceResult
            {
                ErrorType = "None",
                Content = JsonConvert.SerializeObject(new ErrorMacroResponse(httpStatusCode, message))
            };
        }
    }
}
