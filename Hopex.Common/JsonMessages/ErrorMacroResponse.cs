using Newtonsoft.Json;
using System.Net;

namespace Hopex.Common.JsonMessages
{
    public class ErrorMacroResponse
    {
        public ErrorMacroResponse() { }
        public ErrorMacroResponse(HttpStatusCode httpStatusCode, string message)
        {
            HttpStatusCode = httpStatusCode;
            Error = message;
        }

        [JsonProperty("httpStatusCode")]
        public HttpStatusCode HttpStatusCode { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
