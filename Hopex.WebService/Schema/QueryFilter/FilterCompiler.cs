
using GraphQL;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;

using Mega.Macro.API.Library;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Hopex.Model.Abstractions;

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
        private IMegaRoot _root;
        private IClassDescription _itemClassDescription;
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IRelationshipDescription _relationship;
        private readonly IModelElement _sourceElement; // Null for root
        private static readonly char[] _forbiddenChars = new char[] { '"', '#' };
        private static readonly int _maxItemsInArray = 50;

        public FilterCompiler(IMegaRoot root, IClassDescription itemClassDescription, IRelationshipDescription relationship, IModelElement source)
        {
            _root = root;
            _itemClassDescription = itemClassDescription;
            _relationship = relationship;
            _sourceElement = source;
        }

        public string CreateHopexQuery(Dictionary<string, object> filter, IMegaObject language)
        {
            if (filter != null)
            {
                VisitExpression(filter, language);
            }
            if (_relationship != null)
            {
                GenerateSourceFilterForSubRelationship(_relationship, filter != null);
            }
            var query = _sb.ToString();
            return query;
        }

        private void GenerateSourceFilterForSubRelationship(IRelationshipDescription queryRelation, bool shouldAddAnd)
        {
            Debug.Assert(_sourceElement != null);
            var reverseRelation = _itemClassDescription.Relationships.Where(r => r.Id == queryRelation.ReverseId).Single();
            if (shouldAddAnd)
            {
                Write(" AND ");
            }
            // On part du principe qu'il n'y a qu'une condition dans le 1er segment et qu'il n'y en aura pas d'autres
            // et qu'il n'y a que deux segments max
            var segment1 = reverseRelation.Path[0];
            var hasCondition = segment1.Condition != null;
            Write($"{segment1.RoleId}[{segment1.RoleName}]:{segment1.TargetSchemaId}[{segment1.TargetSchemaName}].");
            if (hasCondition)
            {
                Write($"({segment1.Condition.RoleId}[{segment1.Condition.RoleName}]:{segment1.Condition.MetaClassId}[{segment1.Condition.MetaClassName}].~310000000D00[AbsoluteIdentifier] = \"~{segment1.Condition.ObjectFilterId}\" AND ");
            }
            if (reverseRelation.Path.Length == 2)
            {
                var segment2 = reverseRelation.Path[1];
                Write($"{segment2.RoleId}[{segment2.RoleName}]:{segment2.TargetSchemaId}[{segment2.TargetSchemaName}].");
            }

            Write($"~310000000D00[AbsoluteIdentifier] = \"{ _sourceElement.Id.ToString() }\" ");
            if (hasCondition)
            {
                Write(")");
            }
        }

        private void VisitExpression(Dictionary<string, object> dict, IMegaObject language, string connector = "AND", bool forceParen = false)
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
                VisitGroup(parser.Current, connector, language);
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

        private void VisitGroup(KeyValuePair<string, object> elem, string defaultConnector, IMegaObject language)
        {
            CheckElement(elem);
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
                        VisitExpression(e, language, connector);
                    }
                }
                return;
            }
            VisitTerm(elem, defaultConnector, language);
        }

        private void VisitTerm(KeyValuePair<string, object> elem, string connector, IMegaObject language)
        {
            var (id, op) = GetOperator(elem.Key);
            if (op.Pattern == "rel")
            {
                GenerateFilterRelationshipPrefix(id, out _itemClassDescription);
                if (elem.Value is IEnumerable<object> e)
                {
                    VisitExpression(e.First() as Dictionary<string, object>, language, connector, true);
                }
                GenerateFilterRelationshipSuffix();
                return;
            }

            var prop = _itemClassDescription.GetPropertyDescription(id);
            if (string.Equals(id, "id", StringComparison.OrdinalIgnoreCase))
            {
                Write(MetaAttributeLibrary.AbsoluteIdentifier);
            }
            else
            {
                var isAttributeWritten = false;
                if (language != null)
                {
                    if (prop.Scope == PropertyScope.Relationship || prop.Scope == PropertyScope.TargetClass)
                    {
                        var collectionDescription = _root.GetCollectionDescription("~msUikEB5iGM3[Component]");
                        foreach (var property in collectionDescription.NativeObject.Properties)
                        {
                            if (property.SameID(property.RootId, prop.Id) && property.SameID(property.LanguageId, language.MegaUnnamedField))
                            {
                                var reverseRelation = _itemClassDescription.Relationships.Single(r => r.Id == _relationship.ReverseId);
                                var segment1 = reverseRelation.Path[0];
                                Write($"{segment1.RoleId}[{segment1.RoleName}]:{segment1.TargetSchemaId}[{segment1.TargetSchemaName}].({property.MegaField}");
                                isAttributeWritten = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var classDescription = _root.GetClassDescription(prop.Owner.Id);
                        foreach (var property in classDescription.NativeObject.Description.Item(1).Properties)
                        {
                            if (property.SameID(property.RootId, prop.Id) && property.SameID(property.LanguageId, language.MegaUnnamedField))
                            {
                                Write(property.MegaField);
                                isAttributeWritten = true;
                                break;
                            }
                        }
                    }
                }
                if(!isAttributeWritten)
                {
                    if (prop.Scope == PropertyScope.Relationship || prop.Scope == PropertyScope.TargetClass)
                    {
                        var reverseRelation = _itemClassDescription.Relationships.Single(r => r.Id == _relationship.ReverseId);
                        var segment1 = reverseRelation.Path[0];
                        Write($"{segment1.RoleId}[{segment1.RoleName}]:{segment1.TargetSchemaId}[{segment1.TargetSchemaName}].(");
                    }
                    WriteAttribute(prop);
                }
            }

            Write($" {op.Name} ");
            if (op.Pattern == "null")
            {
                WriteRightStatementNull((bool)elem.Value);
            }
            else
            {
                WriteRightStatement(op, elem.Value, prop);
            }

            if (prop.Scope == PropertyScope.Relationship || prop.Scope == PropertyScope.TargetClass)
            {
                Write($"AND ~310000000D00[AbsoluteIdentifier] = \"{_sourceElement.Id}\")");
            }
        }

        private void GenerateFilterRelationshipPrefix(string relationshipName, out IClassDescription targetClass)
        {
            var rel = _itemClassDescription.GetRelationshipDescription(relationshipName);
            bool first = true;
            foreach (var segment in rel.Path)
            {
                if (first)
                {
                    // On part du principe qu'il n'y a qu'une condition dans le 1er segment et qu'il n'y en aura pas d'autres
                    Write($"{segment.RoleId}[{segment.RoleName}]:{segment.TargetSchemaId}[{segment.TargetSchemaName}].(");
                    if (segment.Condition != null)
                    {
                        Write($"{segment.Condition.RoleId}[{segment.Condition.RoleName}]:{segment.Condition.MetaClassId}[{segment.Condition.MetaClassName}].~310000000D00[AbsoluteIdentifier] = \"~{segment.Condition.ObjectFilterId}\" AND ");
                    }
                    first = false;
                }
                else
                {
                    Write($"{segment.RoleId}[{segment.RoleName}]:{segment.TargetSchemaId}[{segment.TargetSchemaName}].");
                } 
            }
            targetClass = rel.TargetClass;
        }

        private void GenerateFilterRelationshipSuffix()
        {
            Write(")");
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
                    CheckValue(e);
                    WriteConstant(e, prop);
                }
            }
            else
            {
                CheckValue(value);
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

        private void WriteRightStatementNull(bool nullValues)
        {
            if(!nullValues)
            {
                Write("Not ");
            }
            Write("Null ");
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
                if(prop.NativeType == typeof(bool))
                {
                    Write((bool)value ? "1" : "0");
                }
                else
                {
                    var valueStr = (value is IConvertible valueConvertible) ? valueConvertible.ToString(CultureInfo.InvariantCulture) : value?.ToString();
                    Write(valueStr);
                }
                Write(delimiter);
                Write(" ");
            }
        }

        private void CheckValue(object value)
        {
            if(value is string valueStr)
            {
                foreach (var character in valueStr)
                {
                    if (_forbiddenChars.Contains(character))
                    {
                        throw new ExecutionError($"Value {valueStr} contains forbidden value: {character}");
                    }
                }
            }
        }

        private void CheckElement(KeyValuePair<string, object> elem)
        {
            if(elem.Value is IEnumerable<object> list)
            {
                if(list.Count() > _maxItemsInArray)
                {
                    throw new ExecutionError($"Number of items in array field [{elem.Key}] should not exceed 50");
                }
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
                { "_none", new HopexOperator("none", "rel") },
                { "_null", new HopexOperator("Is", "null") }
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
