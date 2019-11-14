using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class IntFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is int valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare == (int)Value;
                    case "_not":
                        return valueToCompare != (int)Value;
                    case "_in":
                        return ((List<object>)Value).Contains(valueToCompare);
                    case "_not_in":
                        return !((List<object>)Value).Contains(valueToCompare);
                    case "_lt":
                        return valueToCompare < (int)Value;
                    case "_lte":
                        return valueToCompare <= (int)Value;
                    case "_gt":
                        return valueToCompare > (int)Value;
                    case "_gte":
                        return valueToCompare >= (int)Value;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
