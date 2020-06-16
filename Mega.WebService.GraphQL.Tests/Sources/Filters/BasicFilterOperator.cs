using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class BasicFilterOperator : FilterOperator
    {
        public BasicFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite, true) {}

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected)
        {
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
                if (Compare(item.GetValue(fieldName) as JValue, result, ignoreCase) == ComparisonResult.EQUALS)
                {
                    expected.Add(item);
                }
            }

            return result;
        }
    }
}
