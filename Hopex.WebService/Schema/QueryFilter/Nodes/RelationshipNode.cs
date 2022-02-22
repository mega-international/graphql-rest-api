using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.Filters;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using Mega.Macro.API.Library;
using System;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class RelationshipNode : Node
    {
        private readonly GraphQLRelationshipDescription _graphQlRelationship;
        private readonly HopexOperator _hopexOperator;
        private readonly object _filterValue;
        private readonly Logical _logical;
        
        private IRelationshipDescription Relationship => _graphQlRelationship.MetaAssociation;
        private Node _node;
        public RelationshipNode(GraphQLRelationshipDescription relationship, HopexOperator hopexOperator, object filterValue, Logical logical)
        {
            _graphQlRelationship = relationship;
            _hopexOperator = hopexOperator;
            _filterValue = filterValue;
            _logical = logical;
        }

        public override void Build()
        {
            var lastPath = Relationship.Path.Last();
            switch(_hopexOperator.Name)
            {
                case "count":
                {
                    _node = new CountNode(lastPath, _filterValue);
                    break;
                }
                case "some":
                {
                    _node = new SomeNode(_graphQlRelationship, _filterValue, _logical);
                    break;
                }
                default:
                {
                    throw new NotSupportedException($"Unsupported operation for relation type: {_hopexOperator.Name}");
                }
            }
            _node.Build();
        }

        public override string GetQuery()
        {
            if(_node is SomeNode)
            {
                return _node.GetQuery();
            }
            var countParenthesis = 0;
            var startPath = "";
            for(int idx = 0; idx < Relationship.Path.Length - 1; ++idx)
            {
                var path = Relationship.Path [idx];
                ++countParenthesis;
                startPath += $"{path.RoleId}[{path.RoleName}]:{path.TargetSchemaId}[{path.TargetSchemaName}].(";
                if(path.Condition != null)
                {
                    startPath += $"{path.Condition.RoleId}[{path.Condition.RoleName}]:{path.Condition.MetaClassId}[{path.Condition.MetaClassName}].{MetaAttributeLibrary.AbsoluteIdentifier} = \"~{path.Condition.ObjectFilterId}\" AND ";
                }
            }
            var endPath = new string(')', countParenthesis);
            return $"{startPath}{_node.GetQuery()}{endPath}";
        }
    }
}
