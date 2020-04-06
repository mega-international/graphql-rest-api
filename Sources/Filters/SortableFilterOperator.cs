using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class SortableFilterOperator : ListableFilterOperator
    {
        public SortableFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite) { }

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out int count)
        {
            var values = new List<JValue>();
            foreach(var item in items)
            {
                var value = item.GetValue(fieldName) as JValue;
                value = GetValue(value, ignoreCase);
                values.Add(value);
            }
            values.Sort();

            //Random value
            Random random = new Random();
            var idx = random.Next(0, values.Count);
            var valueFilter = values [idx];

            //Generate results
            if(Name == "_lt")
            {
                count = values.IndexOf(valueFilter);
            }
            else
            {
                count = values.Count - (values.LastIndexOf(valueFilter) + 1);
            }
            return valueFilter;
        }
    }
}