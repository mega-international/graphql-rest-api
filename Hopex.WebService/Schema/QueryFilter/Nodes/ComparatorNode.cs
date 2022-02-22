using Hopex.Modules.GraphQL.Schema.Filters;
using System;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class ComparatorNode : Node
    {
        private readonly string _attributeName;
        private readonly HopexOperator _hopexOperator;
        private readonly object _value;
        private readonly Type _valueType;

        private AttributeNameNode _leftChild;
        private Node _rightChild;

        public ComparatorNode(string attributeName, HopexOperator hopexOperator, object value, Type valueType)
        {
            _attributeName = attributeName;
            _hopexOperator = hopexOperator;
            _value = value;
            _valueType = valueType;
        }

        public override void Build()
        {
            _leftChild = new AttributeNameNode(_attributeName);
            if(_hopexOperator.Pattern == "null")
            {
                _rightChild = new NullableNode((bool)_value);
            }
            else
            {
                _rightChild = new ConstantValueNode(_value, _hopexOperator.Pattern, _valueType);
            }
            _leftChild.Build();
            _rightChild.Build();
        }

        public override string GetQuery()
        {
            return $"{_leftChild.GetQuery()} {_hopexOperator.Name} {_rightChild.GetQuery()}";
        }
    }
}
