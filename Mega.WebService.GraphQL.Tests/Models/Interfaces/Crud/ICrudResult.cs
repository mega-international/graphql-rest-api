using Newtonsoft.Json.Linq;

namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud
{
    public interface ICrudResult
    {
        bool Ok { get; }
        JToken Result { get; }
    }
}
