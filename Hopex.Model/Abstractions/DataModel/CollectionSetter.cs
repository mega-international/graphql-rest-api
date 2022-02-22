using GraphQL;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
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
        private static readonly string [] _unsettableNames = new string [] { "id", "idType", "creationMode" };

        private readonly Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> _resolver;

        public IRelationshipDescription RelationshipDescription { get; }
        public CollectionAction Action { get; }
        public IEnumerable<object> ListElement { get; }

        public static CollectionSetter Create(IRelationshipDescription relationshipDescription,
                                            CollectionAction action,
                                            IEnumerable<object> listElements,
                                            Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> resolver) =>
            new CollectionSetter(relationshipDescription, action, listElements, resolver);

        protected CollectionSetter(IRelationshipDescription relationshipDescription,
                                CollectionAction action,
                                IEnumerable<object> listElements,
                                Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> resolver)
        {
            RelationshipDescription = relationshipDescription;
            Action = action;
            ListElement = listElements;
            _resolver = resolver;
        }

        public async Task UpdateElementAsync(IHopexDataModel model, IModelElement source)
        {
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
                    RemoveAllConnection(source, link);
                    foreach (var element in elements)
                    {
                        await ActionAdd(model, source, link, element);
                    }
                    break;
                case CollectionAction.Add:
                    if (!permissions.IsCreatable)
                    {
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                    }
                    foreach (var element in elements)
                    {
                        await ActionAdd(model, source, link, element);
                    }
                    break;
                case CollectionAction.Remove:
                    if (!permissions.IsDeletable)
                    {
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                    }
                    foreach (var element in elements)
                    {
                        await ActionRemove(source, link, element);
                    }
                    break;
            }
        }

        protected virtual CrudResult GetPathCrud(IModelElement source, IPathDescription path)
        {
            return CrudComputer.GetPathCrud(source.IMegaObject, path);
        }

        private void RemoveConnection(IModelElement source, IModelElement target, IRelationshipDescription relationship)
        {
            var hasInterElements = false;
            var current = target;
            while(current.Parent != source) //Il faudra supprimer les objets intermédiaires
            {
                hasInterElements = true;
                var parent = current.Parent;
                parent.IMegaObject.Delete("NoHierarchy");
                current = parent;
            }

            if(!hasInterElements) //Il faudra rompre le lien direct
            {
                var firstPath = relationship.Path[0];
                if(!CanRemoveConnection(source.IMegaObject, target.IMegaObject))
                {
                    throw new ExecutionError($"You are not allowed to perform this action on this property ({firstPath.RoleName})");
                }
                var collection = source.IMegaObject.GetCollection(firstPath.RoleId);
                collection.RemoveChild(Utils.NormalizeHopexId(target.Id));
            }
        }

        private void RemoveAllConnection(IModelElement source, IRelationshipDescription relationship)
        {
            bool hasInterObjects = relationship.Path.Length > 1;
            if (hasInterObjects)
            {
                var interObjects = new List<IMegaObject>();
                var currents = new List<IMegaObject> { source.IMegaObject };
                for(int idx = 0; idx < relationship.Path.Length - 1; ++idx)
                {
                    var path = relationship.Path [idx];
                    var targets = new List<IMegaObject>();
                    foreach(var current in currents)
                    {
                        targets.AddRange(current.GetCollection(path.RoleId));
                    }
                    interObjects.AddRange(targets);
                    currents = targets;
                }
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if (relationship.Path.Length == 1)
            {
                RemoveAllLinks(source.IMegaObject, relationship.Path[0]);
            }
        }        

        private async Task ActionAdd(IHopexDataModel model, IModelElement source, IRelationshipDescription relationship, Dictionary<string, object> properties)
        {
            string id = null;
            var idType = IdTypeEnum.INTERNAL;
            if(properties.ContainsKey("id"))
            {
                id = properties["id"].ToString();
            }
            if(properties.ContainsKey("idType"))
            {
                Enum.TryParse(properties ["idType"].ToString(), out idType);
            }

            if(id != null) //présence de l'id => c'est juste un connect classique
            {
                var elemSchema = relationship.TargetClass;
                var elemToInsert = await model.GetElementByIdAsync(elemSchema, id, idType);
                if(elemToInsert == null) //Element target inexistant, il faut le créer
                {
                    switch(idType)
                    {
                        case IdTypeEnum.INTERNAL:
                            throw new ExecutionError($"{id}' is not a valid item to add for relationship {relationship.Name} of {id}");
                        case IdTypeEnum.EXTERNAL:
                            await CreateItemInRelationshipAsync(source, relationship, properties, id, idType); //external doit être créé
                            return;
                        case IdTypeEnum.TEMPORARY:
                            throw new ExecutionError($"{id}' is not a valid item to add for relationship {relationship.Name} of {id}");
                        default:
                            return;
                    }
                }
                else //Il faut linker et updater
                {
                    await InsertConnectionAsync(source, elemToInsert, relationship, properties);
                }
            }
            else // pas d'id => il faut créer l'objet linké
            {
                if(idType == IdTypeEnum.EXTERNAL || idType == IdTypeEnum.TEMPORARY)
                {
                    throw new ExecutionError("Parameter id must be set");
                }
                await CreateItemInRelationshipAsync(source, relationship, properties, null, IdTypeEnum.INTERNAL);
            }
        }

        private async Task ActionRemove(IModelElement source, IRelationshipDescription relationship, Dictionary<string, object> properties)
        {
            string id = null;
            var idType = IdTypeEnum.INTERNAL;
            if(properties.ContainsKey("id"))
            {
                id = properties ["id"].ToString();
            }
            if(properties.ContainsKey("idType"))
            {
                Enum.TryParse(properties ["idType"].ToString(), out idType);
            }

            if(string.IsNullOrEmpty(id))
            {
                throw new ExecutionError($"id must be set to remove a link");
            }

            var elementToUnlink = await FindExistingElementFromSource(source, id, idType, relationship);
            if(elementToUnlink != null)
            {
                RemoveConnection(source, elementToUnlink, relationship);
            }
        }

        private async Task CreateItemInRelationshipAsync(IModelElement source, IRelationshipDescription relationship, Dictionary<string, object> properties, string id, IdTypeEnum idType)
        {
            bool useInstanceCreator = false;
            if(properties.TryGetValue("creationMode", out var useInstanceCreatorObj))
            {
                useInstanceCreator = (bool)useInstanceCreatorObj;
            }

            var newElement = await source.CreateElementAsync(relationship, id, idType, useInstanceCreator, CreateSetters(relationship.TargetClass, properties));
            if(newElement.Errors?.Any() ?? false) //Get all errors from new item to report them if any occurs
            {
                source.AddErrors(newElement);
            }
        }

        protected virtual async Task<IModelElement> FindExistingElementFromSource(IModelElement source, MegaId idToFind, IdTypeEnum idType, IRelationshipDescription relationship)
        {
            return await source.GetElementByIdAsync(relationship, idToFind.ToString(), idType);
        }

        private async Task InsertConnectionAsync(IModelElement source, IModelElement elementToLink, IRelationshipDescription relationship, Dictionary<string, object> properties)
        {
            var setters = CreateSetters(relationship.TargetClass, properties);
            //On le fait depuis la source pour vérifier si l'objet est déjà linké
            var existingLinked = await FindExistingElementFromSource(source, elementToLink.Id, IdTypeEnum.INTERNAL, relationship);
            if(existingLinked == null) //Si le link n'existe pas, il faut linker
            {
                if(!CanCreateConnection(source.IMegaObject, elementToLink.IMegaObject))
                {
                    throw new ExecutionError($"You are not allowed to perform this action on this property ({relationship.Path[0].RoleName})");
                }
                bool useInstanceCreator = false;
                if(properties.TryGetValue("creationMode", out var useInstanceCreatorObj))
                {
                    useInstanceCreator = (bool)useInstanceCreatorObj;
                }
                await source.LinkElementAsync(relationship, useInstanceCreator, elementToLink, setters);
            }
            else //Le lien existe, on update seulement
            {
                await existingLinked.UpdateAsync(setters);
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

        void RemoveAllLinks(IMegaObject source, IPathDescription path)
        {
            var collection = source.GetCollection(path.RoleId);
            collection.Select(o =>
                {
                    if (!CanRemoveConnection(source, o))
                        throw new ExecutionError($"You are not allowed to perform this action on this property ({path.RoleName})");
                    return o.Id;
                })
                .ToList()
                .ForEach(id => collection.RemoveChild(id));
        }

        private IEnumerable<ISetter> CreateSetters(IClassDescription entity, IDictionary<string, object> properties)
        {
            var propertiesSettable = properties.Where(p => IsSetterName(p.Key)).ToDictionary(p => p.Key, p => p.Value);
            return _resolver(entity, propertiesSettable);
        }

        private bool IsSetterName(string propertyName)
        {
            return !_unsettableNames.Any(uname => propertyName.Equals(uname, StringComparison.OrdinalIgnoreCase));
        }
    }
}
