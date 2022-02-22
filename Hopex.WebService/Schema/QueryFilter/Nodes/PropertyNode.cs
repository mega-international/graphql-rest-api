using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.Filters;
using Hopex.Modules.GraphQL.Schema.GraphQLSchema;
using Mega.Macro.API.Library;
using System;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    internal class PropertyNode : Node
    {
        private readonly GraphQLRelationshipDescription _graphQlRelationshipParent;
        private readonly GraphQLPropertyDescription _graphQlProperty;
        private readonly HopexOperator _hopexOperator;
        private readonly object _filterValue;

        private ComparatorNode _node;
        private IPropertyDescription Property => _graphQlProperty.MetaAttribute;

        public PropertyNode(GraphQLRelationshipDescription graphQlRelationship, GraphQLPropertyDescription graphQlProperty, HopexOperator hopexOperator, object filterValue)
        {
            _graphQlRelationshipParent = graphQlRelationship;
            _graphQlProperty = graphQlProperty;
            _hopexOperator = hopexOperator;
            _filterValue = filterValue;
        }

        public override void Build()
        {
            var attributeName = $"{Property.Id}[{Property.Name}]";
            if(string.Equals(Property.Name, "id", StringComparison.OrdinalIgnoreCase))
            {
                attributeName = MetaAttributeLibrary.AbsoluteIdentifier;
            }
            _node = new ComparatorNode(attributeName, _hopexOperator, _filterValue, Property.NativeType);
            _node.Build();
        }

        public override string GetQuery()
        {
            if(_graphQlRelationshipParent == null)
            {
                return _node.GetQuery();
            }
            else
            {
                var pathes = _graphQlProperty.GetPathesToField(_graphQlRelationshipParent);
                var pathBegin = "";
                var conditionCount = 0;
                foreach(var path in pathes)
                {
                    pathBegin += $"{path.RoleId}[{path.RoleName}]:{path.TargetSchemaId}[{path.TargetSchemaName}].";
                    if(path.Condition != null)
                    {
                        ++conditionCount;
                        pathBegin += $"({path.Condition.RoleId}[{path.Condition.RoleName}]:{path.Condition.MetaClassId}[{path.Condition.MetaClassName}].{MetaAttributeLibrary.AbsoluteIdentifier} = \"~{path.Condition.ObjectFilterId}\" AND ";
                    }
                }
                var pathEnd = new string(')', conditionCount);
                return $"{pathBegin}{_node.GetQuery()}{pathEnd}";

            }
        }
    }
}
