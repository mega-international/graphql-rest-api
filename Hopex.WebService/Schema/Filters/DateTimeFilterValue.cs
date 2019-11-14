using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public class DateTimeFilterValue : FilterValue
    {
        public override bool Compare(object value)
        {
            if(value is DateTime valueToCompare && Value != null && Value is DateTime filterValue)
            {
                switch(Operation)
                {
                    case "":
                        return valueToCompare.Date == filterValue.Date;
                    case "_not":
                        return valueToCompare.Date != filterValue.Date;
                    case "_in":
                        if (Value is List<object> listValueAsObjectIn)
                        {
                            foreach (var filterValueAsObject in listValueAsObjectIn)
                            {
                                if (filterValueAsObject is DateTime filterDate &&
                                    valueToCompare.Date == filterDate.Date)
                                {
                                    return true;
                                }
                            }
                        }
                        return false;
                    case "_not_in":
                        if (Value is List<object> listValueAsObjectNotIn)
                        {
                            foreach (var filterValueAsObject in listValueAsObjectNotIn)
                            {
                                if (filterValueAsObject is DateTime filterDate &&
                                    valueToCompare.Date != filterDate.Date)
                                {
                                    return true;
                                }
                            }
                        }
                        return false;
                    case "_lt":
                        return valueToCompare.Date < filterValue.Date;
                    case "_lte":
                        return valueToCompare.Date <= filterValue.Date;
                    case "_gt":
                        return valueToCompare.Date > filterValue.Date;
                    case "_gte":
                        return valueToCompare.Date >= filterValue.Date;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
