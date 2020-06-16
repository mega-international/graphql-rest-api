using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class SegmentableFilterOperator : FilterOperator
    {
        public SegmentableFilterOperator(string name, string oppositeName, bool isOpposite)
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
            var randomWord = randomIdx < values.Count ? values.Keys[randomIdx].ToString() : "";
            var start = Name == "_starts_with" ? 0 : random.Next(0, randomWord.Length+1);
            var end = Name == "_ends_with" ? randomWord.Length : random.Next(start, randomWord.Length+1);

            //Result
            var resultStr = randomWord.Substring(start, end - start);

            //Expected
            expected = new List<JObject>();
            foreach (var item in items)
            {
                var itemStr = item.GetValue(fieldName).ToString();
                if ((Name == "_starts_with" && itemStr.StartsWith(resultStr, StringComparison.OrdinalIgnoreCase)) ||
                    (Name == "_ends_with" && itemStr.EndsWith(resultStr, StringComparison.OrdinalIgnoreCase)) ||
                    (Name == "_contains" && itemStr.IndexOf(resultStr,StringComparison.OrdinalIgnoreCase) != -1))
                {
                    expected.Add(item);
                }
            }

            return new JValue(resultStr);
        }
    }
}
