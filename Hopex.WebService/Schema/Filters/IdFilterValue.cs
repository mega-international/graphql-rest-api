using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class IdFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is string valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return string.Equals(valueToCompare, Value.ToString(), StringComparison.Ordinal);
                    case "_not":
                        return !string.Equals(valueToCompare, Value.ToString(), StringComparison.Ordinal);
                    case "_in":
                        return ((List<object>)Value).FindIndex(x => x.ToString().Equals(valueToCompare, StringComparison.Ordinal)) != -1;
                    case "_not_in":
                        return ((List<object>)Value).FindIndex(x => x.ToString().Equals(valueToCompare, StringComparison.Ordinal)) == -1;
                    default:
                        return false;
                }
            }
            return Operation.Contains("_not");
        }
    }
}
