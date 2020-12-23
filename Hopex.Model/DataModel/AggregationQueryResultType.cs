using System.Collections.Generic;

namespace Hopex.Model.DataModel
{
    public class AggregationQueryResultType
    {
        public int Count { get; set; }
        public Dictionary<string, double> AggregatedValues { get; set; } = new Dictionary<string, double>();
    }
}
