
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;

using Mega.Macro.API.Library;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    class FilterParser
    {
        private readonly IEnumerator<KeyValuePair<string, object>> _filter;

        public bool HasMany { get; }

        public FilterParser(Dictionary<string, object> filter)
        {
            _filter = filter.GetEnumerator();
            HasMany = filter.Count > 1;
        }

        public KeyValuePair<string, object> Current => _filter.Current;

        public bool NextToken()
        {
            return _filter.MoveNext();
        }
    }

    internal class FilterCompiler
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IClassDescription _itemClassDescription;
        private readonly IRelationshipDescription _relationship;
        private readonly IModelElement _sourceElement; // Null for root

        public FilterCompiler(IClassDescription itemClassDescription, IRelationshipDescription relationship, IModelElement source)
        {
            _itemClassDescription = itemClassDescription;
            _relationship = relationship;
            _sourceElement = source;
        }

        public string CreateHopexQuery(Dictionary<string, object> filter)
        {
            if (filter != null)
            {
                VisitExpression(filter);
            }
            if (_relationship != null)
            {
                GenerateSourceFilterForSubRelationship(_relationship, filter != null);
            }
            var query = _sb.ToString();
            return query;
        }

        private void GenerateSourceFilterForSubRelationship(IRelationshipDescription rel, bool shouldAddAnd)
        {
            Debug.Assert(_sourceElement != null);

            // On transforme l'id de la relation (si + de un saut)
            string rid = rel.Id;
            var parts = rid.Split('_');
            if (parts.Length > 1)
            {
                // id = part1_part2_part3_part4
                //part1 = MAE + Metaclass
                //part2 = MAE + Mtaclass
                //part 3 id condition
                //Part 4 : target condition
                //transformer en part2_part1_part3_part4
                // En gros on inverse les 2 premiers
                var tmp = parts[0];
                parts[0] = parts[1];
                parts[1] = tmp;
                rid = String.Join("_", parts);
            }
            // Recherche de la relation avec l'id modifié
            var relationships = _itemClassDescription.Relationships.Where(r => r.Id == rid && r.Path.Last().TargetSchemaId == _sourceElement.ClassDescription.Id);
            Debug.Assert(relationships.Count() == 1);
            var rel2 = relationships.First();
            if (shouldAddAnd)
            {
                Write(" AND ");
            }
            // On part du principe qu'il n'y a qu'une condition dans le 1er segment et qu'il n'y en aura pas d'autres
            // et qu'il n'y a que deux segments max
            var segment1 = rel2.Path[0];
            var hasCondition = segment1.Condition != null;
            Write($"{segment1.RoleId}[{segment1.RoleName}]:{segment1.TargetSchemaId}[{segment1.TargetSchemaName}].");
            if (hasCondition)
            {
                Write($"({segment1.Condition.RoleId}[{segment1.Condition.RoleName}]:{segment1.Condition.MetaClassId}[{segment1.Condition.MetaClassName}].~310000000D00[AbsoluteIdentifier] = \"~{segment1.Condition.ObjectFilterId}\" AND ");
            }
            if (rel2.Path.Length == 2)
            {
                var segment2 = rel2.Path[1];
                Write($"{segment2.RoleId}[{segment2.RoleName}]:{segment2.TargetSchemaId}[{segment2.TargetSchemaName}].");
            }

            Write($"~310000000D00[AbsoluteIdentifier] = \"{ _sourceElement.Id.ToString() }\" ");
            if (hasCondition)
            {
                Write(")");
            }
        }

        private void VisitExpression(Dictionary<string, object> dict, string connector = "AND", bool forceParen = false)
        {
            bool first = true;
            var parser = new FilterParser(dict);
            if (parser.HasMany || forceParen)
            {
                Write("(");
            }
            while (parser.NextToken())
            {
                if (!first)
                {
                    WriteOperator(connector);
                }
                first = false;
                VisitGroup(parser.Current, connector);
            }
            if (parser.HasMany || forceParen)
            {
                Write(")");
            }
        }

        private void WriteOperator(string op)
        {
            Write(" ");
            Write(op);
            Write(" ");
        }

        private void VisitGroup(KeyValuePair<string, object> elem, string defaultConnector)
        {
            var connector = elem.Key.ToLower();
            if (connector == "and" || connector == "or")
            {
                if (elem.Value is Dictionary<string, object>)
                {
                    throw new ArrayRequiredException("And & Or requires an array");
                }
                if (elem.Value is IEnumerable<object> list)
                {
                    bool first = true;
                    foreach (Dictionary<string, object> e in list)
                    {
                        if (!first)
                        {
                            WriteOperator(connector);
                        }
                        first = false;
                        VisitExpression(e, connector);
                    }
                }
                return;
            }
            VisitTerm(elem, defaultConnector);
        }

        private void VisitTerm(KeyValuePair<string, object> elem, string connector)
        {
            var (id, op) = GetOperator(elem.Key);
            if (op.Pattern == "rel")
            {
                GenerateFilterRelationshipPrefix(id);
                if (elem.Value is IEnumerable<object> e)
                {
                    VisitExpression(e.First() as Dictionary<string, object>, connector, true);
                }
                return;
            }

            var prop = _itemClassDescription.GetPropertyDescription(id);
            if (string.Equals(id, "id", StringComparison.OrdinalIgnoreCase))
            {
                Write(MetaAttributeLibrary.AbsoluteIdentifier);
            }
            else
            {
                WriteAttribute(prop);
            }

            Write($" {op.Name} ");
            WriteRightStatement(op, elem.Value, prop);
        }

        private void GenerateFilterRelationshipPrefix(string relationshipId)
        {
            var rel = _itemClassDescription.GetRelationshipDescription(relationshipId);
            bool first = true;
            foreach (var segment in rel.Path)
            {
                if (first)
                {
                    // On part du principe qu'il n'y a qu'une condition dans le 1er segment et qu'il n'y en aura pas d'autres
                    if (segment.Condition != null)
                    {
                        Write($"{segment.RoleId}[{segment.RoleName}]:{segment.TargetSchemaId}[{segment.TargetSchemaName}].{segment.Condition.RoleId}[{segment.Condition.RoleName}]:{segment.Condition.MetaClassId}[{segment.Condition.MetaClassName}].~310000000D00[AbsoluteIdentifier] = \"~{segment.Condition.ObjectFilterId}\" AND ");
                    }
                    first = false;
                }
                Write($"{segment.RoleId}[{segment.RoleName}]:{segment.TargetSchemaId}[{segment.TargetSchemaName}].");
            }
        }

        private void Write(string txt)
        {
            _sb.Append(txt);
        }

        private void WriteRightStatement(HopexOperator op, object value, IPropertyDescription prop)
        {
            if (value is IEnumerable<object> list)
            {
                foreach (var e in list)
                {
                    WriteConstant(e, prop);
                }
            }
            else
            {
                if (op.Pattern != null)
                {
                    WriteConstant(string.Format(op.Pattern, value), prop);
                }
                else
                {
                    WriteConstant(value, prop);
                }
            }
        }

        private void WriteAttribute(IPropertyDescription prop)
        {
            Write($"{prop.Id}[{prop.Name}]");
        }

        private void WriteConstant(object value, IPropertyDescription prop)
        {
            if (prop.NativeType == typeof(DateTime))
            {
                var dateTimeValue = (DateTime)value;
                if (dateTimeValue.TimeOfDay == TimeSpan.Zero)
                {
                    value = "~G2H9KK2qI100{D}[" + dateTimeValue.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture) + "]";
                }
                else
                {
                    value = "~Wbl9vR2qI100{T}[" + dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + "]";
                }
                Write($"{value} ");
            }
            else
            {
                const string delimiter = @"""";
                Write(delimiter);
                Write(value?.ToString());
                Write(delimiter);
                Write(" ");
            }
        }

        private (string, HopexOperator) GetOperator(string key)
        {
            var operatorNames = new Dictionary<string, HopexOperator> {
                { "_not", new HopexOperator("Not=", null, true) },
                { "_not_in", new HopexOperator("not in", null, true) },
                { "_in", new HopexOperator("in", null) },
                { "_lt", new HopexOperator("<", null) },
                { "_lte", new HopexOperator("<=", null) },
                { "_gt",new HopexOperator(">", null) },
                { "_gte", new HopexOperator(">=", null) },
                { "_not_contains", new HopexOperator("not Like", "#{0}#", true) },
                { "_contains", new HopexOperator("Like", "#{0}#") },
                { "_not_starts_with", new HopexOperator("not Like", "{0}#", true) },
                { "_starts_with", new HopexOperator("Like", "{0}#") },
                { "_not_ends_with", new HopexOperator("not Like", "#{0}", true) },
                { "_ends_with", new HopexOperator("Like", "#{0}") },
                { "_some", new HopexOperator("some", "rel") },
                { "_every", new HopexOperator("every", "rel") },
                { "_none", new HopexOperator("none", "rel") }
            };

            foreach (var op in operatorNames)
            {
                if (key.EndsWith(op.Key))
                {
                    return (key.Substring(0, key.Length - op.Key.Length), op.Value);
                }
            }
            return (key, HopexOperator.EqualOperator);
        }
    }
}
