using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
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
        public static CollectionSetter Create(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> listElements) => new CollectionSetter(relationshipDescription, action, listElements);

        private CollectionSetter(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> listElements)
        {
            RelationshipDescription = relationshipDescription;
            Action = action;
            _list = listElements;
        }

        public IEnumerable<MutationListElement> Elements => _list.Cast<Dictionary<string, object>>().Select(x=> new MutationListElement{ Id = x["id"].ToString() });
        public IRelationshipDescription RelationshipDescription { get; }
        public CollectionAction Action { get; }

        private readonly IEnumerable<object> _list;
    }

    public class CollectionActionGraphType : EnumerationGraphType<CollectionAction>
    {
        public CollectionActionGraphType()
        {
            Name = "_InputCollectionActionEnum";
        }
    }

    public class MutationAction
    {
        public CollectionAction Action { get; set; }
        public List<MutationListElement> List { get; set; }
    }

    public class MutationActionGraphType : InputObjectGraphType<MutationAction>
    {
        public MutationActionGraphType()
        {
            Name = "_MutationAction";
            Field<NonNullGraphType<CollectionActionGraphType>>("action", resolve: o => o.Source.Action);
            Field<ListGraphType<NonNullGraphType<MutationListElementGraphType>>>("list", resolve: o => o.Source.List);
        }
    }

    public class MutationListElement
    {
        public string Id { get; set; }
    }

    public class MutationListElementGraphType : InputObjectGraphType<MutationListElement>
    {
        public MutationListElementGraphType()
        {
            Name = "_MutationListElement";
            Field("id", o => o.Id);
        }
    }
}
