using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class CurrentContextForMutationType : ObjectGraphType<CurrentContextForMutationResultType>
    {
        public CurrentContextForMutationType()
        {
            Field(x => x.Language);
        }
    }
}
