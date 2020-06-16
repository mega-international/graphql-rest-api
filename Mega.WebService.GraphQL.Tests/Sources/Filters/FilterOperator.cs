using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Filters
{
    public enum ComparisonResult
    {
        LOWER,
        EQUALS,
        HIGHER,
        INCOMPARABLE
    }

    public abstract class FilterOperator
    {
        public readonly string Name;
        public readonly string OppositeName;
        public readonly bool IsOpposite;
        protected readonly bool _canBeIdentifier;

        protected FilterOperator(string name,
                              string oppositeName,
                              bool isOpposite,
                              bool canBeIdentifier)
        {
            Name = name;
            OppositeName = oppositeName;
            IsOpposite = isOpposite;
            _canBeIdentifier = canBeIdentifier;
        }

        public bool CanBeIdentifier()
        {
            return _canBeIdentifier;
        }

        public JToken GenerateValueFilter(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected)
        {
            JToken result;
            if (IsOpposite)
            {
                result = GenerateValueFilterByOpposite(items, fieldName, ignoreCase, out expected);
            }
            else
            {
                result = GenerateValueFilterByBase(items, fieldName, ignoreCase, out expected);
            }
            ByPass(ref expected, fieldName);
            return result;
        }

        public abstract JToken GenerateValueFilterByBase(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected);

        protected JToken GenerateValueFilterByOpposite(List<JObject> items, string fieldName, bool ignoreCase, out List<JObject> expected)
        {
            var baseOperator = FilterOperators.GetFilterOperatorByName(OppositeName);
            var baseValue = baseOperator.GenerateValueFilterByBase(items, fieldName, ignoreCase, out var baseExpected);
            expected = GetOppositeExpected(items, baseExpected, fieldName);
            return baseValue;
        }

        protected virtual List<JObject> GetOppositeExpected(List<JObject> items, List<JObject> baseExpected, string fieldName)
        {
            if(baseExpected == null)
            {
                return null;
            }
            var result = new List<JObject>(items);
            result.RemoveAll(item => baseExpected.Exists(baseItem => baseItem.GetValue(fieldName) == item.GetValue(fieldName)));
            return result;
        }

        protected JValue GetValue(JValue value)
        {
            if(value.Type == JTokenType.String)
            {
                var strValue = value.ToString();
                return new JValue(strValue);
            }
            return value;
        }

        protected SortedList<JValue, int> GetSortedValues(List<JObject> items, string fieldName)
        {
            var values = new SortedList<JValue, int>();
            foreach (var item in items)
            {
                var value = item.GetValue(fieldName) as JValue;
                value = GetValue(value);
                if (!values.ContainsKey(value) && value.Type != JTokenType.Null)
                {
                    values.Add(value, 0);
                }
            }
            return values;
        }

        protected ComparisonResult Compare(JValue value1, JValue value2, bool ignoreCase)
        {
            int compareInt;
            if(value1.Type == JTokenType.String && value2.Type == JTokenType.String)
            {
                var value1str = value1.ToString();
                var value2str = value2.ToString();
                var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                compareInt = string.Compare(value1str, value2str, comparisonMode);
            }
            else if(value1.Type == JTokenType.Null && value2.Type == JTokenType.Null)
            {
                compareInt = 0;
            }
            else if(value1.Type != JTokenType.Null && value2.Type != JTokenType.Null)
            {
                compareInt = value1.CompareTo(value2);
            }
            else
            {
                return ComparisonResult.INCOMPARABLE;
            }
            return compareInt < 0 ? ComparisonResult.LOWER : (compareInt > 0 ? ComparisonResult.HIGHER : ComparisonResult.EQUALS);
        }

        protected void ByPass(ref List<JObject> expected, string fieldName)
        {
            expected?.RemoveAll(item =>
            {
                var value = item.GetValue(fieldName);
                if (value.Type == JTokenType.Null)
                    return true;
                else if (value.Type == JTokenType.String && value.ToString() == "")
                    return true;
                return false;
            });
        }
    }
}
