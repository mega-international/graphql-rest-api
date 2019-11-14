using System.Collections.Generic;
using System.Linq;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public enum CollectionAction
    {
        Add,
        Remove,
        ReplaceAll
    }

    public class CollectionSetter : ISetter
    {
        public static CollectionSetter Create(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> ids) => new CollectionSetter(relationshipDescription, action, ids);

        private CollectionSetter(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> ids)
        {
            RelationshipDescription = relationshipDescription;
            Action = action;
            _list = ids;
        }

        public IEnumerable<string> Ids => _list.Cast<string>();
        public IRelationshipDescription RelationshipDescription { get; }
        public CollectionAction Action { get; }

        private readonly IEnumerable<object> _list;
    }
}
