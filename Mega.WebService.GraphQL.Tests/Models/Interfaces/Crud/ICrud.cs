using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud
{
    public static class QueryType
    {
        public const string Query = "query";
        public const string Mutation = "query";
    }

    public interface ICrud
    {
        Task<ICrudResult> Query(string queryType, List<ICrudOutput> outputs, List<ICrudFragment> fragments, bool asyncMode);
        ICrudOutput Create(string metaclassName, ICrudInput input, string mode, List<ICrudOutput> outputs);
        ICrudOutput Read(string metaclassName, List<ICrudInput> inputs, List<ICrudOutput> outputs);
        ICrudOutput Update(string metaclassName, string id, ICrudInput input, List<ICrudOutput> outputs);
        ICrudOutput Delete(string metaclassName, string id, bool cascade);
    }
}
