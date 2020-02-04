using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class ListableFilterOperator : BasicFilterOperator
    {
        public ListableFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite) { }

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out int count)
        {
            var countByValue = new SortedList<JValue, int>();
            foreach(var item in items)
            {
                var value = item.GetValue(fieldName) as JValue;
                value = GetValue(value, ignoreCase);
                if(countByValue.TryGetValue(value, out int valueCount))
                {
                    countByValue [value] = valueCount + 1;
                }
                else
                {
                    countByValue.Add(value, 1);
                }
            }
            //Random indexes
            Random random = new Random();
            var chosenCount = random.Next(0, 1+Math.Min(countByValue.Count, 25));

            //Generate results
            JArray valuesArray = new JArray();
            count = 0;
            while(chosenCount > 0)
            {
                var idx = random.Next(0, countByValue.Count);
                valuesArray.Add(countByValue.Keys [idx]);
                count += countByValue.Values [idx];
                countByValue.RemoveAt(idx);
                --chosenCount;
            }
            return valuesArray;
        }
    }
}
