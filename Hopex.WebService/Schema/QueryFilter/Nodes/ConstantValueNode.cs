using GraphQL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class ConstantValueNode : Node<object>
    {
        private static readonly char [] _forbiddenChars = new char [] { '"', '#' };
        private readonly Type _constantType;
        private readonly string _pattern;

        private object _resolvedValue;

        public ConstantValueNode(object constantValue, string pattern, Type constantType) : base(constantValue)
        {
            _constantType = constantType;
            _pattern = pattern;
        }

        public override void Build()
        {
            if(Value is IEnumerable<object> list)
            {
                foreach(var item in list)
                {
                    CheckValue(item);
                }
            }
            else
            {
                CheckValue(Value);
            }
            ResolveValue();
        }

        public override string GetQuery()
        {
            if(_resolvedValue is IEnumerable<object> list)
            {
                var result = "";
                var isFirst = true;
                foreach(var item in list)
                {
                    if(!isFirst)
                    {
                        result += " ";
                    }
                    isFirst = false;
                    result += GetItemStr(item);
                }
                return result;
            }
            else
            {
                return GetItemStr(_resolvedValue);
            }
        }

        private void ResolveValue()
        {
            if(_pattern != null)
            {
                _resolvedValue = string.Format(_pattern, Value);
            }
            else
            {
                _resolvedValue = Value;
            }
        }

        private string GetItemStr(object item)
        {
            if(_constantType == typeof(DateTime))
            {
                return GetDateTimeStr((DateTime)item);
            }
            else
            {
                if(_constantType == typeof(bool))
                {
                    return GetBooleanStr((bool)item);
                }
                else if(_constantType == typeof(IntCount))
                {
                    return item.ToString();
                }
                else
                {
                    return $"\"{((item is IConvertible valueConvertible) ? valueConvertible.ToString(CultureInfo.InvariantCulture) : item?.ToString())}\"";
                }
            }
        }

        private string GetDateTimeStr(DateTime dateTimeValue)
        {
            if(dateTimeValue.TimeOfDay == TimeSpan.Zero)
            {
                return "~G2H9KK2qI100{D}[" + dateTimeValue.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture) + "]";
            }
            else
            {
                return "~Wbl9vR2qI100{T}[" + dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + "]";
            }
        }

        private string GetBooleanStr(bool boolValue)
        {
            return boolValue ? "\"1\"" : "\"0\"";
        }



        private void CheckValue(object toCheck)
        {
            if(toCheck is string valueStr)
            {
                foreach(var character in valueStr)
                {
                    if(_forbiddenChars.Contains(character))
                    {
                        throw new ExecutionError($"Value {valueStr} contains forbidden value: {character}");
                    }
                }
            }
        }
    }
}
