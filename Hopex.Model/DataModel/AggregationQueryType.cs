using GraphQL.Types;

namespace Hopex.Model.DataModel
{
    public class AggregationQueryType : ObjectGraphType
    {
        public AggregationQueryResultType AggregationQueryResultType { get; set; }
    }
}
