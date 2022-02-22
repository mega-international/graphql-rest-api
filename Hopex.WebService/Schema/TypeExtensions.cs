using GraphQL;
using GraphQL.Types;
using Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL.Schema
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Resolves a Type to its equivalent GraphType.
        /// </summary>
        public static Type ToGraphType(this Type type, bool forceNullable)
        {
            try
            {
                // Already a graph type
                if (type.IsGraphType())
                    return type;

                // Unwrap a Task type
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                    type = type.GetGenericArguments()[0];

                // Collection types
                var enumerableType = type.IsArray ? type.GetElementType() : type.GetEnumerableType();
                if (enumerableType != null)
                    return typeof(ListGraphType<>).MakeGenericType(enumerableType.ToGraphType(false));

                // Nullable value types
                var nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null)
                    return GetGraphTypeInternal(nullableType);

                // Value types
                if (type.GetTypeInfo().IsValueType && forceNullable)
                    return typeof(NonNullGraphType<>).MakeGenericType(GetGraphTypeInternal(type));

                // Everything else
                return GetGraphTypeInternal(type);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException($"Unsupported type: {type.Name}", exception);
            }
        }

        /// <summary>
        /// Gets the type T for a type implementing IEnumerable&lt;T&gt;.
        /// </summary>
        public static Type GetEnumerableType(this Type type)
        {
            if (type == typeof(string))
                return null;

            if (type.IsGenericEnumerable())
                return type.GetGenericArguments()[0];

            return type
                .GetInterfaces()
                .Where(t => t.IsGenericEnumerable())
                .Select(t => t.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        private static bool IsGenericEnumerable(this Type type)
        {
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }


        /// <summary>
        /// Resolve the GraphType for a Type, first looking for any attributes
        /// then falling back to the default library implementation.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetGraphTypeInternal(Type type)
        {
            // Support enum types
            if (type.GetTypeInfo().IsEnum)
                return typeof(EnumerationGraphType<>).MakeGenericType(type);

            return GetGraphTypeFromType(type);
        }

        public static Type GetGraphTypeFromType(Type type)
        {
            Type propertyType;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DateTime:
                    propertyType = typeof(CustomDateGraphType);
                    break;
                case TypeCode.Decimal:
                    propertyType = typeof(CustomDecimalGraphType);
                    break;
                case TypeCode.Double:
                case TypeCode.Single:
                    propertyType = typeof(CustomFloatGraphType);
                    break;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    propertyType = typeof(CustomIntGraphType);
                    break;
                default:
                    propertyType = type.GetGraphTypeFromType(true);
                    break;
            }
            return propertyType;
        }
    }
}
