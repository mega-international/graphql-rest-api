using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.DataModel
{
    public class CustomRelationshipSetter : CollectionSetter
    {
        public CustomRelationshipSetter(IRelationshipDescription relationshipDescription, CollectionAction action, List<object> list)
            :base(relationshipDescription, action, list)
        {
        }

        internal override void UpdateLinkAttributes(HopexModelElement elemToInsert, IRelationshipDescription relationShip, Dictionary<string, object> propertyValues, MegaObject current)
        {
        }

        protected override IClassDescription GetTargetSchema(IModelElement source, IRelationshipDescription link)
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

        protected override bool FillInterObjects(MegaObject source, MegaObject target, IPathDescription[] paths, ref List<MegaObject> interObjects, int idx = 0)
        {
            return Action == CollectionAction.Remove;
        }

        private static CustomRelationshipSetter Create(Dictionary<string, object> prop)
        {
            var relationId = prop["relationId"].ToString();
            var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), prop["action"].ToString(), true);
            var list = (List<object>)prop["list"];
            var relationshipDescription = new RelationshipDescription(relationId, null, null, relationId, "");
            var linkDescritpion = new PathDescription(relationId, relationId, relationId, null, null, null, null);
            relationshipDescription.SetPath(new PathDescription[] { linkDescritpion });
            return new CustomRelationshipSetter(relationshipDescription, action, list);
        }

        public static IEnumerable<ISetter> CreateSetters(object values)
        {
            var props = (List<object>)values;
            foreach (var prop in props)
            {
                yield return Create((Dictionary<string, object>)prop);
            }           
        }


    }
}
