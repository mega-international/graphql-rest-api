using Hopex.ApplicationServer.WebServices;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;

namespace Hopex.Modules.GraphQL.Schema
{

    public interface ISchemaManagerProvider
    {
        Task<GraphQLSchemaManager> GetInstanceAsync(IHopexContext hopexContext, string version, IMegaRoot MegaRoot, ILogger logger);
    }
}
