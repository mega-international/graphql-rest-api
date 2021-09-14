using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;

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

    internal class ObjectWithParentGraphType<TSourceType> : ObjectGraphType<TSourceType>
    {
        private readonly ObjectGraphType<TSourceType> _parentGraphType = null;
        private bool _extended = false;

        internal ObjectWithParentGraphType() : this(null) {}

        internal ObjectWithParentGraphType(ObjectGraphType<TSourceType> parentGraphType)
        {
            _parentGraphType = parentGraphType;
        }

        internal void Extend()
        {
            Extend(null);
        }

        internal void Extend(IEnumerable<FieldType> overridedFields)
        {
            if (!_extended)
            {
                if (_parentGraphType != null)
                {
                    //On fait hériter tous les champs, les champs à surcharger sont dans "overridedFields"
                    overridedFields = overridedFields ?? new List<FieldType>();
                    var fields = _parentGraphType.Fields.Except(overridedFields, new FieldComparer()).Concat(overridedFields);
                    foreach (var field in fields)
                    {
                        AddField(field);
                    }
                    //On fait hériter de toutes les interfaces de la classe mère
                    foreach(var resolvedInterface in _parentGraphType.ResolvedInterfaces)
                    {
                        AddResolvedInterface(resolvedInterface);
                    }
                }
                _extended = true;
            }
        }
    }
}
