using GraphQL;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class FilterNode : Node<IDictionary<string, object>>
    {
        private static readonly int _maxItemsInArray = 50;

        private readonly GraphQLRelationshipDescription _graphQLRelationshipParent;
        private readonly GraphQLClassDescription _graphQlClass;
        private readonly Logical _logical;
        private readonly IModelElement _source = null;
        private readonly GraphQLRelationshipDescription _graphQlRelationship = null;
        
        private readonly List<Node> _childs = new List<Node>();

        public FilterNode(GraphQLRelationshipDescription graphQLRelationship,
            GraphQLClassDescription graphQlClass,
            object filterValue,
            Logical logical,
            IModelElement source = null,
            GraphQLRelationshipDescription graphQlRelationship = null) : base(filterValue)
        {
            _graphQLRelationshipParent = graphQLRelationship;
            _graphQlClass = graphQlClass;
            _logical = logical;
            _source = source;
            _graphQlRelationship = graphQlRelationship;
        }

        public override void Build()
        {
            _childs.AddRange(Value.Select(pair => CreateNode(pair.Key, pair.Value)));
            if(_source != null && _graphQlRelationship != null)
            {
                _childs.Add(new RelationshipNode(_graphQlRelationship.Reverse,
                    GetOperator("_some").Item2,
                    new List<Dictionary<string, object>> { new Dictionary<string, object> { { "id", _source.Id.ToString() } } },
                    _logical));
            }
            foreach(var child in _childs)
            {
                child.Build();
            }
        }

        public override string GetQuery()
        {
            return CreateQueryWithSeparator(_logical, _childs, c => c.GetQuery());
        }

        private Node CreateNode(string fieldName, object fieldValue)
        {
            CheckElement(fieldName, fieldValue);
            if(LogicalNode.IsLogicalWord(fieldName))
            {
                var logical = LogicalNode.GetLogicalFromWord(fieldName);
                return new LogicalNode(_graphQLRelationshipParent, _graphQlClass, logical, fieldValue);
            }
            else
            {
                var (baseFieldName, op) = GetOperator(fieldName);
                if(op.Pattern == "rel")
                {
                    var relationship = _graphQlClass.Relationships.FirstOrDefault(r => r.Name.Equals(baseFieldName, StringComparison.OrdinalIgnoreCase));
                    return new RelationshipNode(relationship, op, fieldValue, _logical);
                }
                else
                {
                    var property = _graphQlClass.Properties.FirstOrDefault(p => p.Name.Equals(baseFieldName, StringComparison.OrdinalIgnoreCase));
                    return new PropertyNode(_graphQLRelationshipParent, property, op, fieldValue);
                }
            }
        }

        private void CheckElement(string fieldName, object fieldValue)
        {
            if(fieldValue is IEnumerable<object> list)
            {
                if(list.Count() > _maxItemsInArray)
                {
                    throw new ExecutionError($"Number of items in array field [{fieldName}] should not exceed {_maxItemsInArray}");
                }
            }
        }
    }
}
