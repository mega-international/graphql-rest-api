using GraphQL.Types;

namespace Hopex.Model.DataModel
{
    public class AggregateFunctionType : EnumerationGraphType<AggregateFunctionTypeEnum>
    {
        public AggregateFunctionType()
        {
            Name = "AggregateFunctionType";
        }
    }
}
