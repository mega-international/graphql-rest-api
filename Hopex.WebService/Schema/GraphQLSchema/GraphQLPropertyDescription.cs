using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.GraphQLSchema
{
    internal class GraphQLPropertyComparer : IEqualityComparer<GraphQLPropertyDescription>
    {
        public bool Equals(GraphQLPropertyDescription x, GraphQLPropertyDescription y)
        {
            return x.Name.Equals(y.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(GraphQLPropertyDescription obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    internal class GraphQLPropertyDescription
    {
        public IPropertyDescription MetaAttribute { get; }
        public string Name { get; }
        private readonly IEnumerable<IPathDescription> _reversePathes;
        private readonly Dictionary<string, IEnumerable<IPathDescription>> _pathesByRelationship = new Dictionary<string, IEnumerable<IPathDescription>>();
        public GraphQLPropertyDescription(IPropertyDescription metaAttrbiute, string name = null)
        {
            MetaAttribute = metaAttrbiute;
            Name = name ?? metaAttrbiute.Name;
        }

        public void AddPathesToField(GraphQLRelationshipDescription graphQLRelationship, IEnumerable<IPathDescription> pathes)
        {
            _pathesByRelationship.Add(graphQLRelationship.MetaAssociation.Id, pathes);
        }

        public IEnumerable<IPathDescription> GetPathesToField(GraphQLRelationshipDescription graphQLRelationship)
        {
            if(_pathesByRelationship.TryGetValue(graphQLRelationship.MetaAssociation.Id, out var pathes))
            {
                return pathes;
            }
            return new List<IPathDescription>();
        }
    }
}
