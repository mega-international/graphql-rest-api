using System;

namespace Hopex.Modules.GraphQL.Dataset
{
    internal static class CharEnumUtils
    {
        internal static T ToCharEnum<T>(string value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Type {typeof(T)} is not an enumeration.");
            foreach (T item in Enum.GetValues(typeof(T)))
                if (((char)Convert.ToByte(item)).ToString() == value)
                    return item;
            throw new ArgumentException($"Unrecognized value {value}");
        }
    }
}
