using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using Mega.Macro.API.Library;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class SomeNode : Node<IEnumerable<object>>
    {
        private readonly GraphQLRelationshipDescription _graphQlRelationship;
        private readonly Logical _logical;

        private readonly List<FilterNode> _childs = new List<FilterNode>();
        private IRelationshipDescription Relationship => _graphQlRelationship.MetaAssociation;

        private GraphQLClassDescription GraphQlClass => _graphQlRelationship.TargetClass;

        public SomeNode(GraphQLRelationshipDescription graphQlRelationship, object filterValue, Logical logical) : base(filterValue)
        {
            _graphQlRelationship = graphQlRelationship;
            _logical = logical;
        }

        public override void Build()
        {
            _childs.AddRange(Value.Select(filter => new FilterNode(_graphQlRelationship, GraphQlClass, filter, _logical)).ToList());
            foreach(var child in _childs)
            {
                child.Build();
            }
        }

        public override string GetQuery()
        {
            var countParenthesis = 0;
            var startPath = "";
            for(int idx = 0;idx < Relationship.Path.Length; ++idx)
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
            return CreateQueryWithSeparator(_logical, _childs, c => $"{startPath}{c.GetQuery()}{endPath}");
        }
    }
}
