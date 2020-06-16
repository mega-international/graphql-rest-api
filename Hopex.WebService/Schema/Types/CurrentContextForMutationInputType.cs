using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class CurrentContextForMutationInputType: InputObjectGraphType
    {
        public CurrentContextForMutationInputType()
        {
            Name = "currentContext";
            Field<NonNullGraphType<LanguagesEnumerationGraphType>>("language");
        }
    }
}
