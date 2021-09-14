using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.MetaModel;
using System;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.DataModel
{
    public class CustomRelationshipSetter : CollectionSetter
    {
        public CustomRelationshipSetter(IRelationshipDescription relationshipDescription, CollectionAction action, IEnumerable<object> list)
            :base(relationshipDescription, action, list)
        {}

        protected override IClassDescription GetTargetSchema(IModelElement source, IPathDescription path)
        {
            return new ClassDescription(source.MetaModel, "GenericRelationTarget", null, null, false);
        }

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

        private static CustomRelationshipSetter Create(Dictionary<string, object> prop)
        {
            var relationId = prop["relationId"].ToString();
            var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), prop["action"].ToString(), true);
            var list = (IEnumerable<object>)prop["list"];
            var relationshipDescription = new RelationshipDescription(relationId, null, null, $"{relationId}[customRelationship]", relationId, "", null)
            {
                TargetClass = new ClassDescription(null, "unknow target", null, null, false)
            };
            var linkDescritpion = new PathDescription(relationId, relationId, relationId, null, null, null, null);
            relationshipDescription.SetPath(new PathDescription[] { linkDescritpion });
            return new CustomRelationshipSetter(relationshipDescription, action, list);
        }

        public static IEnumerable<ISetter> CreateSetters(object values)
        {
            var props = (IEnumerable<object>)values;
            foreach (var prop in props)
            {
                yield return Create((Dictionary<string, object>)prop);
            }           
        }
    }
}
