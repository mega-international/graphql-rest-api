using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public class SegmentableFilterOperator : SortableFilterOperator
    {
        public SegmentableFilterOperator(string name, string oppositeName, bool isOpposite)
            : base(name, oppositeName, isOpposite) { }

        public override JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out int count)
        {
            var values = new List<string>();
            for(var segmentLength = 2; segmentLength > 0 && values.Count <= 0; --segmentLength)
            {
                foreach(var item in items)
                {
                    var value = item.GetValue(fieldName) as JValue;
                    value = GetValue(value, ignoreCase);
                    var strValue = value.ToString();
                    if(strValue.Length >= segmentLength)
                    {
                        var segments = new HashSet<string>();
                        var start = (Name == "_ends_with" ? strValue.Length - segmentLength : 0);
                        var end = (Name == "_starts_with" ? segmentLength : strValue.Length);
                        for(var idx = start;idx + segmentLength <= end;++idx)
                        {
                            var segment = strValue.Substring(idx, segmentLength);
                            segments.Add(segment);
                        }
                        values.AddRange(segments);
                    }
                }
            }
            values.Sort();

            //Random value
            Random random = new Random();
            var rndIdx = random.Next(0, values.Count);
            var rndValue = values [rndIdx];

            //Generate results
            count = values.LastIndexOf(rndValue) - values.IndexOf(rndValue) + 1;
            return rndValue;
        }
    }
}
