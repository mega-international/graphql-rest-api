using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class SortableFilterOperator : FilterOperator
    {
        public SortableFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite, false) { }

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected)
        {
            //Values
            var values = GetSortedValues(items, fieldName);
            if (values.Count == 0)
            {
                expected = null;
                return null;
            }

            //Random value
            Random random = new Random();
            var randomIdx = random.Next(0, values.Count);

            //Result
            var result = values.Keys[randomIdx];

            //Expected
            expected = new List<JObject>();
            foreach (var item in items)
            {
                ComparisonResult comparison = Compare(item.GetValue(fieldName) as JValue, result, ignoreCase);
                if((comparison == ComparisonResult.HIGHER && Name == "_gt") || (comparison == ComparisonResult.LOWER && Name == "_lt"))
                {
                    expected.Add(item);
                }
            }

            return result;
        }

        protected override List<JObject> GetOppositeExpected(List<JObject> items, List<JObject> baseExpected, string fieldName)
        {
            var result = base.GetOppositeExpected(items, baseExpected, fieldName);
            result?.RemoveAll(item => item.GetValue(fieldName).Type == JTokenType.Null);
            return result;
        }
    }
}
