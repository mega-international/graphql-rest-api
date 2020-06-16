using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class ListableFilterOperator : FilterOperator
    {
        public ListableFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite, true) { }

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected)
        {
            //Values
            var values = GetSortedValues(items, fieldName);
            if(values.Count == 0)
            {
                expected = null;
                return null;
            }
            
            //Random count
            Random random = new Random();
            var randomCount = random.Next(1, Math.Min(values.Count+1, 20));

            //Result
            JArray result = new JArray();
            while(randomCount > 0)
            {
                var randomIdx = random.Next(0, randomCount);
                result.Add(values.Keys[randomIdx]);
                values.RemoveAt(randomIdx);
                --randomCount;
            }
            if(result.Count == 0)
            {
                result.Add(new JValue(""));
            }

            //Expected
            expected = new List<JObject>();
            var resultList = result.ToObject<List<JValue>>();
            foreach (var item in items)
            {
                if (resultList.Exists(resultItem => Compare(resultItem, item.GetValue(fieldName) as JValue, ignoreCase) == ComparisonResult.EQUALS))
                {
                    expected.Add(item);
                }
            }

            return result;
        }
    }
}
