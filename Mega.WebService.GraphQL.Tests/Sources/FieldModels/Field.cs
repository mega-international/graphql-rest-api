using Mega.WebService.GraphQL.Tests.Sources.FieldModels.Classes;
using Mega.WebService.GraphQL.Tests.Sources.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    [Flags]
    public enum Kind : byte
    {
        Unknown,
        NonNull,
        Scalar  = NonNull << 1,
        List    = Scalar << 1,
        Interface = List << 1,
        Object  = Interface << 1,
        Enum    = Object << 1,
        All     = byte.MaxValue
    }

    public abstract class Field
    {
        public static Kind GetKindByName(string kindName)
        {
            switch(kindName)
            {
                case "NON_NULL":
                    return Kind.NonNull;
                case "SCALAR":
                    return Kind.Scalar;
                case "LIST":
                    return Kind.List;
                case "INTERFACE":
                    return Kind.Interface;
                case "OBJECT":
                case "INPUT_OBJECT":
                    return Kind.Object;
                case "ENUM":
                    return Kind.Enum;
            }
            return Kind.Unknown;
        }

        public Field(string name, bool nullable = true)
        {
            Name = name;
            Nullable = nullable;
            if(Name != null)
            {
                _operator = FilterOperators.GetFilterOperatorByFieldName(Name);
            }
        }

        public readonly string Name;

        public readonly bool Nullable;

        public bool IsIdentifier()
        {
            var originalName = GetOriginalName();
            string[] idNames = { "id", "creatorId", "modifierId", "currentStateId" };
            return Array.Exists(idNames, idName => idName == originalName);
        }

        protected readonly FilterOperator _operator;
        public abstract string GetStringFormat(JToken token);    

        public virtual JToken GenerateValueFilter(List<JObject> items, out List<JObject> expected)
        {
            var fieldName = GetOriginalName();
            return _operator.GenerateValueFilter(items, fieldName, !IsIdentifier(), out expected);
        }

        public string GetOutputFormat(bool showName = true, object parameters = null)
        {
            if(parameters is FieldsParameters fieldsParameters)
            {
                return GetOutputFormatInternal(showName, fieldsParameters.Get(Name)?.Parameters);
            }
            else if(parameters is FieldParameters fieldParameters)
            {
                return GetOutputFormatInternal(showName, fieldParameters.Parameters);
            }
            else if(parameters is null)
            {
                return GetOutputFormatInternal(showName, null);
            }
            else
            {
                throw new ArgumentException($"Parameters has invalid type: {parameters.GetType()}");
            }
        }

        protected virtual string GetOutputFormatInternal(bool showName, IReadOnlyDictionary<string, string> parameters)
        {
            var parametersStr = parameters == null ? "" : BuildParametersString(parameters);
            return showName ? (Name + parametersStr) : "";
        }

        public string GetOriginalName()
        {
            if(!string.IsNullOrEmpty(_operator.Name))
            {
                var index = Name.LastIndexOf(_operator.Name);
                return Name.Remove(index);
            }
            return Name;
        }

        public bool IsOriginalName()
        {
            return (_operator?.Name ?? "?") == "";
        }
        
        public override string ToString()
        {
            var typeName = GetType().Name;
            return $"{typeName}: Name: {Name}, {(Nullable ? "" : "non null")}";
        }

        protected string BuildParametersString(IReadOnlyDictionary<string, string> parameters)
        {
            if(parameters == null)
            {
                throw new ArgumentNullException("Parameters must be non null");
            }
            if(parameters.Count == 0)
            {
                return "";
            }
            var parametersStr = "(";
            bool first = true;
            foreach(var parameter in parameters)
            {
                if(!first)
                {
                    parametersStr += ", ";
                }
                parametersStr += $"{parameter.Key}: {parameter.Value}";
                first = false;
            }
            parametersStr += ")";
            return parametersStr;
        }
    }
}
