using System.Net;

namespace Mega.WebService.GraphQL.Models
{
    public class GraphQlResponse
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Result { get; set; }
        public string Error { get; set; }
    }
}
