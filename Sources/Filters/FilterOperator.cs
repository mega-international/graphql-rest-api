using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public abstract class FilterOperator
    {
        public readonly string Name;
        public readonly string OppositeName;
        public readonly bool IsOpposite;

        public FilterOperator(string name,
                              string oppositeName,
                              bool isOpposite)
        {
            Name = name;
            OppositeName = oppositeName;
            IsOpposite = isOpposite;
        }

        public JToken GenerateValueFilter(List<JObject> items, string fieldName, bool ignoreCase, out int count)
        {
            if(IsOpposite)
            {
                return GenerateValueFilterByOpposite(items, fieldName, ignoreCase, out count);
            }
            return GenerateValueFilterByBase(items, fieldName, ignoreCase, out count);
        }

        public abstract JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out int count);

        protected JToken GenerateValueFilterByOpposite(List<JObject> items, string fieldName, bool ignoreCase, out int count)
        {
            var baseOperator = FilterOperators.GetFilterOperatorByName(OppositeName);
            var baseValue = baseOperator.GenerateValueFilterByBase(items, fieldName, ignoreCase, out int baseCount);
            count = items.Count - baseCount;
            return baseValue;
        }

        protected JValue GetValue(JValue value, bool ignoreCase)
        {
            if(value.Type == JTokenType.String)
            {
                var strValue = value.ToString();
                return new JValue(strValue);
            }
            return value;
        }
    }
}
