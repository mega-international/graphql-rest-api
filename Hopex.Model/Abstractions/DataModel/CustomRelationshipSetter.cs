using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public class CustomRelationshipSetter : CollectionSetter
    {
        public CustomRelationshipSetter(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> list,
            Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> resolver)
            :base(relationshipDescription, action, list, resolver)
        {}

        protected override CrudResult GetPathCrud(IModelElement source, IPathDescription path)
        {
            return CrudComputer.GetCollectionMetaPermission(source.IMegaObject.Root, path.RoleId);
        }

        protected override bool CanCreateConnection(IMegaObject source, IMegaObject child)
        {
            var crud = CrudComputer.GetCollectionCrud(source, RelationshipDescription.RoleId, child);
            return crud.IsCreatable;
        }

        protected override bool CanRemoveConnection(IMegaObject source, IMegaObject child)
        {
            var crud = CrudComputer.GetCollectionCrud(source, RelationshipDescription.RoleId, child);
            return crud.IsDeletable;
        }

        private static CustomRelationshipSetter Create(IDictionary<string, object> props, Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> resolver)
        {
            var relationId = props ["relationId"].ToString();
            var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), props["action"].ToString(), true);
            var list = (IEnumerable<object>)props["list"];
            var relationshipDescription = new CustomRelationshipDescription(relationId);
            return new CustomRelationshipSetter(relationshipDescription, action, list, resolver);
        }

        public static IEnumerable<ISetter> CreateSetters(object values)
        {
            if(values is Tuple<object, Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>>> pair)
            {
                var resolver = pair.Item2;
                if(pair.Item1 is IEnumerable<object> listValues)
                {
                    foreach(var value in listValues)
                    {
                        if(value is IDictionary<string, object> props)
                        {
                            yield return Create(props, resolver);
                        }
                    }
                }
            }        
        }

        protected override Task<IModelElement> FindExistingElementFromSource(IModelElement source, MegaId idToFind, IdTypeEnum idType, IRelationshipDescription relationship)
        {
            var collection = source.GetGenericCollection(relationship.Id);
            return Task.FromResult(collection.FirstOrDefault(e => e.Id.ToString() == idToFind.ToString()));
        }
    }
}
