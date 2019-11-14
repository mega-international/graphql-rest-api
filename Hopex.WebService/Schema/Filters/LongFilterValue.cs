using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class LongFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is long valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare == (long)Value;
                    case "_not":
                        return valueToCompare != (long)Value;
                    case "_in":
                        return ((List<object>)Value).Contains(valueToCompare);
                    case "_not_in":
                        return !((List<object>)Value).Contains(valueToCompare);
                    case "_lt":
                        return valueToCompare < (long)Value;
                    case "_lte":
                        return valueToCompare <= (long)Value;
                    case "_gt":
                        return valueToCompare > (long)Value;
                    case "_gte":
                        return valueToCompare >= (long)Value;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
