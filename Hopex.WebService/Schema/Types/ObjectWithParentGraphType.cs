using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    internal class FieldComparer : IEqualityComparer<FieldType>
    {
        public bool Equals(FieldType x, FieldType y)
        {
            return x?.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public int GetHashCode(FieldType obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    internal class ObjectWithParentGraphType<TSourceType> : ObjectGraphType<TSourceType> {}
}
