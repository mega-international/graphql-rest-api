using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.GraphQLSchema
{
    internal class GraphQLRelationshiComparer : IEqualityComparer<GraphQLRelationshipDescription>
    {
        public bool Equals(GraphQLRelationshipDescription x, GraphQLRelationshipDescription y)
        {
            return x.Name.Equals(y.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(GraphQLRelationshipDescription obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    internal class GraphQLRelationshipDescription
    {
        public GraphQLClassDescription TargetClass { get; }
        public IRelationshipDescription MetaAssociation { get; }
        public string Name => MetaAssociation.Name;
        public GraphQLRelationshipDescription Reverse => TargetClass.Relationships.FirstOrDefault(r => r.MetaAssociation.Id == MetaAssociation.ReverseId);

        public GraphQLRelationshipDescription(IRelationshipDescription metaAssociation, GraphQLClassDescription targetClass)
        {
            MetaAssociation = metaAssociation;
            TargetClass = targetClass;
        }
    }
}
