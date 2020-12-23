using GraphQL.Types;

namespace Hopex.Model.DataModel
{
    public class DeleteType : ObjectGraphType<DeleteResultType>
    {
        public DeleteType()
        {
            Field(x => x.DeletedCount);
        }
    }
}
