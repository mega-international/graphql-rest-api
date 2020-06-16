using System.Web;
using System.Web.Mvc;

namespace Mega.WebService.GraphQL.Tests
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
