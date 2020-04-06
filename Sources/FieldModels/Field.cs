using Mega.WebService.GraphQL.Tests.Sources.Filters;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
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

        protected readonly FilterOperator _operator;
        public abstract string GetStringFormat(JToken token);    

        public virtual JToken GenerateValueFilter(List<JObject> items, out int count)
        {
            var fieldName = GetOriginalName();
            return _operator.GenerateValueFilter(items, fieldName, !IsIdentifier(), out count);
        }

        public virtual string GetOutputFormat(bool showName = true)
        {
            return showName ? Name : "";
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
        
        public bool IsIdentifier()
        {
            return Name == MetaFieldNames.id ||
                   Name == MetaFieldNames.externalIdentifier ||
                   Name == MetaFieldNames.hexaIdAbs;
        }
        public override string ToString()
        {
            var typeName = GetType().Name;
            return $"{typeName}: Name: {Name}, {(Nullable ? "" : "non null")}";
        }

    }
}
