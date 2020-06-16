using Newtonsoft.Json;
using System;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public class ExceptionContent
    {
        [JsonProperty("message")]
        private string _message;

        [JsonProperty("stackTrace")]
        private string _stackTrace;

        [JsonProperty("source")]
        private string _source;
        public ExceptionContent(Exception exception)
        {
            _message = exception.Message;
            _stackTrace = exception.StackTrace;
            _source = exception.Source;
        }
    }
}
