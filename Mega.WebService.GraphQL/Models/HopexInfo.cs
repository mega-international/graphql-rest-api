using System.Web.Mvc;

namespace Mega.WebService.GraphQL.Models
{
    public class HopexInfo
    {
        public string ServiceUrl { get; set; }
        public SelectList Schemas { get; set; }
        public string UasUrl { get; set; }
        public string EnvironmentId { get; set; }
        public string RepositoryId { get; set; }
        public string ProfileId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
