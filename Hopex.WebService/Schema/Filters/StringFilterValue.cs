using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class StringFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is string valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return string.Equals(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    case "_not":
                        return !string.Equals(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    case "_in":
                        return ((List<object>)Value).FindIndex(x => x.ToString().Equals(valueToCompare, StringComparison.OrdinalIgnoreCase)) != -1;
                    case "_not_in":
                        return ((List<object>)Value).FindIndex(x => x.ToString().Equals(valueToCompare, StringComparison.OrdinalIgnoreCase)) == -1;
                    case "_lt":
                        return string.Compare(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase) < 0;
                    case "_lte":
                        return string.Compare(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase) <= 0;
                    case "_gt":
                        return string.Compare(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase) > 0;
                    case "_gte":
                        return string.Compare(valueToCompare, Value.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
                    case "_contains":
                        return valueToCompare.IndexOf(Value.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
                    case "_not_contains":
                        return !(valueToCompare.IndexOf(Value.ToString(), StringComparison.OrdinalIgnoreCase) >= 0);
                    case "_starts_with":
                        return valueToCompare.StartsWith(Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    case "_not_starts_with":
                        return !valueToCompare.StartsWith(Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    case "_ends_with":
                        return valueToCompare.EndsWith(Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    case "_not_ends_with":
                        return !valueToCompare.EndsWith(Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
