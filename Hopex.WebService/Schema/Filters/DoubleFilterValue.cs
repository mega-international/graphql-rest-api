using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class DoubleFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is double valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare.Equals((double)Value);
                    case "_not":
                        return !valueToCompare.Equals((double)Value);
                    case "_in":
                        return ((List<object>)Value).Contains(valueToCompare);
                    case "_not_in":
                        return !((List<object>)Value).Contains(valueToCompare);
                    case "_lt":
                        return valueToCompare < (double)Value;
                    case "_lte":
                        return valueToCompare <= (double)Value;
                    case "_gt":
                        return valueToCompare > (double)Value;
                    case "_gte":
                        return valueToCompare >= (double)Value;
                    default:
                        return false;
                }
            }
            return Operation.Contains("_not");
        }
    }
}
