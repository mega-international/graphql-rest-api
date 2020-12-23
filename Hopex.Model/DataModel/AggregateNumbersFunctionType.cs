using GraphQL.Types;

namespace Hopex.Model.DataModel
{
    public class AggregateNumbersFunctionType : EnumerationGraphType<AggregateNumbersFunctionTypeEnum>
    {
        public AggregateNumbersFunctionType()
        {
            Name = "AggregateNumbersFunctionType";
        }
    }
}
