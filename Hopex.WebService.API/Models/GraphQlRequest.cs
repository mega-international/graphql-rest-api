using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hopex.WebService.API.Models
{
    public class GraphQlRequest
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; } = Guid.NewGuid();

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
