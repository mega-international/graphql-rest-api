using Hopex.Modules.GraphQL.Schema.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    abstract class Node
    {
        protected readonly static Tuple<Logical, string> [] _logicals = new Tuple<Logical, string> []
        {
            new Tuple<Logical, string>(Logical.AND, "AND"),
            new Tuple<Logical, string>(Logical.OR, "OR"),
        };

        protected static string LogicalStr(Logical logical)
        {
            return _logicals.First(pair => pair.Item1 == logical).Item2;
        }

        protected static readonly IDictionary<string, HopexOperator> _operatorNames = new Dictionary<string, HopexOperator> {
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
                { "_count", new HopexOperator("count", "rel") },
                { "_every", new HopexOperator("every", "rel") },
                { "_none", new HopexOperator("none", "rel") },
                { "_empty", new HopexOperator("Is", "null") }
            };

        public abstract void Build();
        public abstract string GetQuery();
        protected static (string, HopexOperator) GetOperator(string key)
        {
            foreach(var op in _operatorNames)
            {
                if(key.EndsWith(op.Key))
                {
                    return (key.Substring(0, key.Length - op.Key.Length), op.Value);
                }
            }
            return (key, HopexOperator.EqualOperator);
        }

        protected static string CreateQueryWithSeparator<T>(Logical logical, IEnumerable<T> items, Func<T, string> itemResolver)
        {
            var manyWithOr = items.Count() > 1 && logical == Logical.OR;
            var queryWithSeparator = manyWithOr ? "(" : "";
            var isFirst = true;
            var separator = LogicalStr(logical);
            foreach(var item in items)
            {
                if(!isFirst)
                {
                    queryWithSeparator += $" {separator} ";
                }
                isFirst = false;
                queryWithSeparator += $"{itemResolver(item)}";
            }
            queryWithSeparator += manyWithOr ? ")" : "";
            return queryWithSeparator;
        }
    }

    abstract class Node<T> : Node
    {
        protected readonly object _filterValue;
        protected virtual T Value
        {
            get
            {
                if(_filterValue is T concreteValue)
                {
                    return concreteValue;
                }
                else
                {
                    throw new NotSupportedException($"{GetType()} cannot handle value of type {_filterValue.GetType()}");
                }
            }
        }

        protected Node(object filterValue)
        {
            _filterValue = filterValue;
        }
    }
}
