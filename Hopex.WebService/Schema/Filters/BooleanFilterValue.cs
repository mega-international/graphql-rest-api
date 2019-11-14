namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class BooleanFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is bool valueToCompare)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare == (bool)Value;
                    case "_not":
                        return valueToCompare != (bool)Value;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
