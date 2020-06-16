using GraphQL.Types;

namespace Hopex.Model.DataModel
{
    public class IdType : EnumerationGraphType<IdTypeEnum>
    {
        public IdType()
        {
            Name = "IdType";
        }
    }
}
