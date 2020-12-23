using GraphQL;
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
    public enum CollectionAction
    {
        Add,
        Remove,
        ReplaceAll
    }

    public class CollectionSetter : ISetter
    {
        public static CollectionSetter Create(IRelationshipDescription relationshipDescription,
                                            CollectionAction action,
                                            IEnumerable<object> listElements) =>
            new CollectionSetter(relationshipDescription, action, listElements);

        protected CollectionSetter(IRelationshipDescription relationshipDescription,
                                CollectionAction action,
                                IEnumerable<object> listElements)
        {
            RelationshipDescription = relationshipDescription;
            Action = action;
            ListElement = listElements;
        }

        public IRelationshipDescription RelationshipDescription { get; }
        public CollectionAction Action { get; }
        public IEnumerable<object> ListElement { get; }

        public async Task UpdateElementAsync(IHopexDataModel model, IModelElement source)
        {
            //_domainModel.LogInformation($"setter col = {cs.ToString()}");
            var link = RelationshipDescription;
            var permissions = GetPathCrud(source, link.Path[0]);
            var elements = ListElement.Cast<Dictionary<string, object>>();
            switch (Action)
            {
                case CollectionAction.ReplaceAll:
                    if (!permissions.IsCreatable || !permissions.IsDeletable)
                    {
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                    }
                    RemoveAllConnection(source, link.Path);
                    foreach (var element in elements)
                    {
                        await ModifyRelationshipAsync(model, source, link, element, true);
                    }
                    break;
                case CollectionAction.Add:
                    if (!permissions.IsCreatable)
                    {
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                    }
                    foreach (var element in elements)
                    {
                        await ModifyRelationshipAsync(model, source, link, element, true);
                    }
                    break;
                case CollectionAction.Remove:
                    if (!permissions.IsDeletable)
                    {
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                    }
                    foreach (var element in elements)
                    {
                        await ModifyRelationshipAsync(model, source, link, element, false);
                    }
                    break;
            }
        }

        protected virtual CrudResult GetPathCrud(IModelElement source, IPathDescription path)
        {
            return CrudComputer.GetPathCrud(source.IMegaObject, path);
        }

        private void RemoveConnection(IModelElement source, IModelElement elemToInsert, IPathDescription[] path)
        {
            var current = source.IMegaObject;
            List<IMegaObject> interObjects = new List<IMegaObject>();
            var found = FillInterObjects(current, elemToInsert.IMegaObject, path, ref interObjects);
            if (!found) //Inexisting link
            {
                return;
            }

            if (interObjects.Any()) //remove interObject will automatically kill links
            {
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if (path.Any()) //remove single link
            {
                IPathDescription hop = path.First();

                if (!CanRemoveConnection(current, elemToInsert.IMegaObject))
                    throw new ExecutionError($"You are not allowed to perform this action on this property ({hop.RoleName})");

                MegaId roleId = hop.RoleId;
                var collection = source.IMegaObject.GetCollection(roleId);
                var id = Utils.NormalizeHopexId(elemToInsert.Id);
                RemoveFromCollection(collection, id);
            }
        }

        private void RemoveAllConnection(IModelElement source, IPathDescription[] paths)
        {
            bool hasInterObjects = paths.Length > 1;
            if (hasInterObjects)
            {
                List<IMegaObject> interObjects = new List<IMegaObject>();
                FillInterObjects(source.IMegaObject, null, paths, ref interObjects);
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if (paths.Length == 1)
            {
                RemoveAllLinks(source.IMegaObject, paths[0]);
            }
        }        

        private bool FillInterObjects(IMegaObject source, IMegaObject target, IPathDescription[] paths, ref List<IMegaObject> interObjects, int idx = 0)
        {
            IMegaObject targetObjectThroughCollection = null;
            return FillInterObjects(source, target, paths, ref interObjects, ref targetObjectThroughCollection, idx);
        }

        private bool FillInterObjects(IMegaObject source, IMegaObject target, IPathDescription[] paths, ref List<IMegaObject> interObjects, ref IMegaObject targetObjectThroughCollection, int idx = 0)
        {
            bool isLast = idx == paths.Length - 1;
            MegaId roleId = paths[idx].RoleId;
            var collection = source.GetCollection(roleId);
            var enumerator = collection.GetEnumerator();
            if ((target == null) && isLast)
            {
                return true;
            }

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                if (isLast) //for last path, we find target id
                {
                    if (item.IsSameId(target.Id) && MetaAssociationConditionFilter(item, paths[idx]))
                    {
                        targetObjectThroughCollection = item;
                        return true;
                    }
                }
                else //for previous paths, we find interobjects
                {
                    if (FillInterObjects(item, target, paths, ref interObjects, ref targetObjectThroughCollection, idx + 1))
                    {
                        if (MetaAssociationConditionFilter(item, paths[idx]))
                        {
                            interObjects.Add(item);
                            if (target != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return (target == null); //target null is always true
        }

        private static bool MetaAssociationConditionFilter(IMegaObject item, IPathDescription hop)
        {
            if (hop.Condition == null || string.IsNullOrEmpty(hop.Condition.RoleId) || string.IsNullOrEmpty(hop.Condition.ObjectFilterId))
            {
                return true;
            }
            return item.GetPropertyValue(Utils.NormalizeHopexId(hop.Condition.RoleId)) == hop.Condition.ObjectFilterId;
        }

        private async Task CreateItemInRelationshipAsync(IHopexDataModel model, IModelElement source, IRelationshipDescription link, Dictionary<string, object> properties, string id, IdTypeEnum idType)
        {
            var elemSchema = GetTargetSchema(source, link);
            bool useInstanceCreator = false;
            if (properties.TryGetValue("creationMode", out var useInstanceCreatorObj))
            {
                useInstanceCreator = (bool)useInstanceCreatorObj;
            }
            var setters = CreateSetters(link.TargetClass, properties);
            var collection = source.IMegaObject.GetCollection(link.RoleId);
            var element = await model.CreateElementFromParentAsync(elemSchema, id, idType, useInstanceCreator, setters, collection);
            if (element.Errors?.Any() ?? false) //Get all errors from new item to report them if any occurs
            {
                source.AddErrors(element);
            }
        }

        private async Task ModifyRelationshipAsync(IHopexDataModel model, IModelElement source, IRelationshipDescription link, Dictionary<string, object> properties, bool insert)
        {
            if(properties.TryGetValue("id", out var idObj)) //présence de l'id => c'est juste un connect classique
            {
                var id = idObj.ToString();
                var idType = IdTypeEnum.INTERNAL;
                if (properties.ContainsKey("idType"))
                {
                    Enum.TryParse(properties["idType"].ToString(), out idType);
                }
                var elemSchema = GetTargetSchema(source, link);
                var getElementByIdAsyncTask = model.GetElementByIdAsync(elemSchema, id, idType);
                if (getElementByIdAsyncTask == null)
                {
                    throw new ExecutionError($"Element {elemSchema.Name} not found with id {id}");
                }

                if (!(await getElementByIdAsyncTask is IModelElement elemToInsert))
                {
                    switch (idType)
                    {
                        case IdTypeEnum.INTERNAL:
                            throw new ExecutionError($"{id}' is not a valid item to add for relationship {link.Name} of {id}");
                        case IdTypeEnum.EXTERNAL:
                            await CreateItemInRelationshipAsync(model, source, link, properties, id, idType); //external doit être créé
                            return;
                        case IdTypeEnum.TEMPORARY:
                            if (model.TemporaryMegaObjects.ContainsKey(id))
                            {
                                elemToInsert = model.TemporaryMegaObjects[id];
                            }
                            else
                            {
                                throw new ExecutionError($"{id}' is not a valid item to add for relationship {link.Name} of {id}");
                            }
                            break;
                        default:
                            return;
                    }
                }
                if (insert)
                    InsertConnection(source, elemToInsert, link, properties);
                else
                    RemoveConnection(source, elemToInsert, link.Path);
            }
            else // pas d'id => il faut créer l'objet linké
            {
                await CreateItemInRelationshipAsync(model, source, link, properties, null, IdTypeEnum.INTERNAL);
            }
        }

        protected virtual IClassDescription GetTargetSchema(IModelElement source, IRelationshipDescription link)
        {
            return source.ClassDescription.MetaModel.GetClassDescription(link.Path.Last().TargetSchemaName);
        }

        private void InsertConnection(IModelElement source, IModelElement elemToInsert, IRelationshipDescription relationShip, Dictionary<string, object> propertyValues)
        {
            var current = source.MegaObject;
            var iCurrent = source.IMegaObject;
            var interObjects = new List<IMegaObject>();
            var path = relationShip.Path;
            IMegaObject targetObjectThroughCollection = null;
            var found = FillInterObjects(iCurrent, elemToInsert.IMegaObject, path, ref interObjects, ref targetObjectThroughCollection);
            if (found) 
            {
                //adding a new connection from a source to a target already linked together is forbidden, but we must update link attributes
                var firstSegmentTarget = interObjects.Count > 0 ? interObjects[0] : targetObjectThroughCollection;
                UpdateLinkAttributes(elemToInsert, relationShip, propertyValues, firstSegmentTarget);
                return;
            }

            if (!CanCreateConnection(source.IMegaObject, elemToInsert.IMegaObject))
                throw new ExecutionError($"You are not allowed to perform this action on this property ({relationShip.Path[0].RoleName})");

            for (int ix = 0; ix < path.Length; ix++)
            {
                bool isLast = ix == path.Length - 1;
                IPathDescription hop = path[ix];

                MegaId roleId = hop.RoleId;

                if (isLast)
                {
                    var collection = iCurrent.GetCollection(roleId);
                    iCurrent = collection.Add(Utils.NormalizeHopexId(elemToInsert.Id));
                    if (iCurrent is RealMegaObject)
                        current = ((RealMegaObject)iCurrent).RealObject;
                }
                else
                {
                    var collection = current.GetCollection(roleId);
                    current = collection.Create();
                    iCurrent = new RealMegaObject(current);
                }
                if (ix == 0)
                {
                    // First segment - Try to set link attributes
                    UpdateLinkAttributes(elemToInsert, relationShip, propertyValues, iCurrent);
                }

                CreateCondition(current, hop.Condition);
            }
        }
        
        private void UpdateLinkAttributes(IModelElement elemToInsert, IRelationshipDescription relationShip, Dictionary<string, object> propertyValues, IMegaObject current)
        {           
            var linkProperties = relationShip.TargetClass.Properties.Where(p => p.Scope == PropertyScope.Relationship || p.Scope == PropertyScope.TargetClass);
            if (linkProperties.Any())
            {
                foreach (var property in linkProperties)
                {
                    foreach (var kv in propertyValues)
                    {
                        if (string.Compare(kv.Key, property.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            var elem = new HopexModelElement(elemToInsert.DomainModel, relationShip.TargetClass, current);
                            elem.SetValue(property, kv.Value);
                            break;
                        }
                    }
                }
            }
        }

        protected virtual bool CanCreateConnection(IMegaObject source, IMegaObject child)
        {
            return true;
        }

        protected virtual bool CanRemoveConnection(IMegaObject source, IMegaObject child)
        {
            return true;
        }

        private void CreateCondition(MegaObject current, PathConditionDescription hopCondition)
        {
            if (hopCondition == null || string.IsNullOrEmpty(hopCondition.RoleId) || string.IsNullOrEmpty(hopCondition.ObjectFilterId))
            {
                return;
            }
            var conditionObject = current.Root.GetObjectFromId<MegaObject>(Utils.NormalizeHopexId(hopCondition.ObjectFilterId));
            current.GetCollection(Utils.NormalizeHopexId(hopCondition.RoleId)).Add(conditionObject.Id);
        }

        void RemoveAllLinks(IMegaObject source, IPathDescription path)
        {
            var collection = source.GetCollection(path.RoleId);
            collection
                .Select(o =>
                {
                    if (!CanRemoveConnection(source, o))
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({path.RoleName})");
                    return o.Id;
                })
                .ToList()
                .ForEach(id => RemoveFromCollection(collection, id));
        }

        private void RemoveFromCollection(IMegaCollection collection, MegaId objectId)
        {
            collection.RemoveChild(objectId);
        }

        private IEnumerable<ISetter> CreateSetters(IClassDescription entity, Dictionary<string, object> properties)
        {
            var result = new List<ISetter>();
            foreach (var kv in properties)
            {
                var prop = entity.GetPropertyDescription(kv.Key, false);
                if (prop != null)
                {
                    result.Add(PropertySetter.Create(prop, kv.Value));
                }
            }
            return result;
        }
    }
}
