using Hopex.ApplicationServer.WebServices;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL.Schema
{

    public interface ISchemaManagerProvider
    {
        Task<GraphQLSchemaManager> GetInstanceAsync(ILogger logger, IHopexContext hopexContext, string version);
    }
}
