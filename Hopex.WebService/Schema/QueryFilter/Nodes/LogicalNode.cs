using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    enum Logical
    {
        AND,
        OR
    }

    class LogicalNode : Node<IEnumerable<object>>
    {
        private readonly GraphQLRelationshipDescription _graphQLRelationshipParent;
        private readonly GraphQLClassDescription _graphQlClass;
        private readonly Logical _logical;

        private readonly List<FilterNode> _childs = new List<FilterNode>();

        public LogicalNode(GraphQLRelationshipDescription graphQLRelationship, GraphQLClassDescription graphQlClass, Logical logical, object filterValue) : base(filterValue)
        {
            _graphQLRelationshipParent = graphQLRelationship;
            _graphQlClass = graphQlClass;
            _logical = logical;
        }

        public override void Build()
        {
            _childs.AddRange(Value.Select(item => new FilterNode(_graphQLRelationshipParent, _graphQlClass, item, _logical)).ToList());
            foreach(var child in _childs)
            {
                child.Build();
            }
        }

        public override string GetQuery()
        {
            return CreateQueryWithSeparator(_logical, _childs, c => c.GetQuery());
        }

        public static Logical GetLogicalFromWord(string word)
        {
            return _logicals.First(pair => pair.Item2.Equals(word, StringComparison.OrdinalIgnoreCase)).Item1;
        }

        public static bool IsLogicalWord(string word)
        {
            return _logicals.Any(pair => pair.Item2.Equals(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}
