using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public static class FilterOperators
    {
        private static readonly List<FilterOperator> _operators = new List<FilterOperator>()
        {
            new SegmentableFilterOperator("_not_contains", "_contains", true),
            new SegmentableFilterOperator("_not_starts_with", "_starts_with", true),
            new SegmentableFilterOperator("_not_ends_with", "_ends_with", true),
            new SortableFilterOperator("_gte", "_lt", true),
            new SortableFilterOperator("_lte", "_gt", true),
            new ListableFilterOperator("_not_in", "_in", true),
            new BasicFilterOperator("_not", "", true),

            new SegmentableFilterOperator("_contains", "_not_contains", false),
            new SegmentableFilterOperator("_starts_with", "_not_starts_with", false),
            new SegmentableFilterOperator("_ends_with", "_not_ends_with", false),
            new SortableFilterOperator("_lt", "_gte", false),
            new SortableFilterOperator("_gt", "_lte", false),
            new ListableFilterOperator("_in", "_not_in", false),
            new BasicFilterOperator("", "_not", false)
        };

        public static FilterOperator GetFilterOperatorByName(string name)
        {
            foreach(var filterOp in _operators)
            {
                if(filterOp.Name == name)
                {
                    return filterOp;
                }
            }
            return null;
        }

        public static FilterOperator GetFilterOperatorByFieldName(string fieldName)
        {
            foreach(var filterOp in _operators)
            {
                if(fieldName.EndsWith(filterOp.Name))
                {
                    return filterOp;
                }
            }
            return null;
        }
    }
}
