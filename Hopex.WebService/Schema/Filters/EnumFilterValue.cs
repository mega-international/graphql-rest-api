using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class EnumFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is string valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare == Value.ToString();
                    case "_not":
                        return valueToCompare != Value.ToString();
                    case "_in":
                        return ((List<string>)Value).Contains(valueToCompare);
                    case "_not_in":
                        return !((List<string>)Value).Contains(valueToCompare);
                    default:
                        return false;
                }
            }
            return Operation.Contains("_not");
        }
    }
}
