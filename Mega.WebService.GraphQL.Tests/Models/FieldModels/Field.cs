using Mega.WebService.GraphQL.Tests.Models.FakeDatas;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.FieldModels
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
                    return Kind.Object;
                case "ENUM":
                    return Kind.Enum;
            }
            return Kind.Unknown;
        }

        public Field(string name, bool nullable = true)
        {
            this.Name = name;
            this.Nullable = nullable;
        }

        public string Name { get; set; }
        public bool Nullable { get; set; }
        public abstract string GetStringFormat(JToken token);

        public virtual string GetOutputFormat()
        {
            return Name;
        }

        public virtual JToken GetFakeValue(Container container, JObject jObj)
        {
            if(!jObj.TryGetValue(Name, out JToken value))
            {
                return null;
            }
            return value;
        }
    }

    public class FieldComparator : IEqualityComparer<Field>
    {
        public bool Equals(Field field1, Field field2)
        {
            return field1?.Name?.Equals(field2?.Name) ?? false;
        }

        public int GetHashCode(Field field)
        {
            return field?.Name?.GetHashCode() ?? 0;
        }
    }
}
