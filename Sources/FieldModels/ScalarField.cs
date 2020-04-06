using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    public class ScalarField : Field
    {
        public string TypeName { get; set; }
        public ScalarField(string name, string typeName, bool nullable = true) : base(name, nullable)
        {
            TypeName = typeName;
        }

        public override string GetStringFormat(JToken token)
        {
            if(token.Type == JTokenType.Null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(token);
        }
        /*
        public override JToken GenerateValueOrderableFilter(List<JObject> items, out int count)
        {
            var values = new List<JValue>();
            var originalName = GetOriginalName();
            foreach(var item in items)
            {
                var value = item.GetValue(originalName) as JValue;
                values.Add(value);
            }
            values.Sort();

            //Random value
            Random rdm = new Random();
            var idx = rdm.Next(0, values.Count);
            var valFilter = values [idx];

            //Get expected count
            var opeName = GetFilterOperatorName();
            count = GetExpectedOrderableCount(values, valFilter);
            return values[idx];
        }

        private int GetExpectedOrderableCount(List<JValue> sortedValues, JValue value, string opeName)
        {
            var opeNames = GetFilterOperatorNames();
            var idxOpe = opeNames.IndexOf(opeName);

            if(idxOpe % 2 == 0)
            {
                var oppositeCount = GetExpectedOrderableCount(sortedValues, value, opeNames [idxOpe + 1]);
                return sortedValues.Count - oppositeCount;
            }

            switch(opeName)
            {
                case "_lt":
                    return sortedValues.IndexOf(value);

                case "_gt":
                    var idxVal = sortedValues.LastIndexOf(value);
                    return sortedValues.Count - idxVal + 1;

                case "":
                    return 1;

                default:
                    return 0;
            }
        }

        public override JToken GenerateValueSegmentableFilter(List<JObject> items, out int count)
        {
            
        }

        private int GetExpectedSegmentableCount(List<JValue> sortedValues, JValue value, string opeName)
        {
            var opeNames = GetFilterOperatorNames();
            var idxOpe = opeNames.IndexOf(opeName);

            if(idxOpe % 2 == 0)
            {
                var oppositeCount = GetExpectedOrderableCount(sortedValues, value, opeNames [idxOpe + 1]);
                return sortedValues.Count - oppositeCount;
            }

            switch(opeName)
            {
                case "_contains":
                    break;

                case "_starts_with":
                    break;

                case "_ends_with":
                    break;

                default:
                    return 0;
            }
        }



        protected override List<string> GetFilterOperatorNames()
        {
            return new List<string>
            {
                "_not_contains",    "_contains",
                "_not_starts_with", "_starts_with",
                "_not_ends_with",   "_ends_with",
                "_gte",             "_lt",
                "_lte",             "_gt",
                "_not",             ""
            };
        }*/

        public override string ToString()
        {
            return base.ToString() + $", typename: {TypeName}";
        }
    }
}
