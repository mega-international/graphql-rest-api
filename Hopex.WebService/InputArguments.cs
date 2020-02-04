using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hopex.Modules.GraphQL
{
    public class InputArguments
    {
        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [Required]
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("variables")]
        public Dictionary<string, object> Variables { get; set; }

        [JsonProperty("webServiceUrl")]
        public string WebServiceUrl { get; set; }
    }
}
