using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.DataModel
{
    internal class HopexModelElement : IModelElement, IDisposable
    {
        private HopexDataModel _domainModel;
        public MegaObject MegaObject { get; }

        public HopexModelElement(HopexDataModel domainModel, IClassDescription schema, MegaObject megaObject, MegaId id = null)
        {
            _domainModel = domainModel;
            ClassDescription = schema;
            MegaObject = megaObject;
            Id = id;
            if(id == null)
            {
                id = Utils.SanitizeId(megaObject.MegaField);
                int pos = id.ToString().IndexOf('[');
                if(pos > 0)
                {
                    id = id.ToString().Substring(0, pos);
                }
            }
            Id = Utils.SanitizeId(id);
        }

        public MegaId Id { get; }
        public IClassDescription ClassDescription { get; }
        public IHopexMetaModel MetaModel => ClassDescription.MetaModel;

        public Task<IModelCollection> GetCollectionAsync(string name, string erql, List<Tuple<string, int>> orderByClauses = null, string relationshipName = null)
        {
            IRelationshipDescription relationshipDescription = ClassDescription.GetRelationshipDescription(relationshipName ?? name);
            return Task.FromResult<IModelCollection>(new HopexModelCollection(_domainModel, relationshipDescription, MegaObject.Root, erql, orderByClauses));
        }

        public T GetValue<T>(string propertyName, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            return GetValue<T>(property, format);
        }

        public void SetValue<T>(string propertyName, T value, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            SetValue<T>(property, value, format);
        }

        public T GetValue<T>(IPropertyDescription property, string format = null)
        {
            var permissions = GetPropertyCrud(property);
            if (! permissions.IsReadable)
            {
                return (T) Convert.ChangeType(null, typeof(T));
            }

            var propertyGetterFormat = format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat;

            switch(property.PropertyType)
            {
                case PropertyType.String:
                case PropertyType.RichText:
                    if(format != null)
                    {
                        if(format == "RAW")
                        {
                            format = "ANSI";
                        }
                        return MegaObject.NativeObject.GetFormated(property.Id.ToString(), format);
                    }
                    return MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                case PropertyType.Enum:
                    var enumResult = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    var q = from x in property.EnumValues
                        where x.InternalValue == enumResult.ToString()
                        select x.Name;
                    return (T)Convert.ChangeType(q.FirstOrDefault(), typeof(T));
                case PropertyType.Date:
                    var dateResult = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    if(dateResult.ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    if(propertyGetterFormat != "ASCII" && MegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return dateResult;
                case PropertyType.Int:
                case PropertyType.Long:
                case PropertyType.Double:
                    var numericResult = MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
                    if(numericResult.ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    if(propertyGetterFormat != "ASCII" && MegaObject.GetPropertyValue<T>(property.Id, "ASCII").ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return numericResult;
                default:
                    return MegaObject.GetPropertyValue<T>(property.Id, propertyGetterFormat);
            }
        }

        public void SetValue<T>(IPropertyDescription property, T value, string format = null)
        {
            var permissions = GetPropertyCrud(property);
            if (! permissions.IsUpdatable)
            {
                throw new ExecutionError($"You are not allowed to perform this action on this property ({property.Name})");
            }
            if(value is DateTime)
            {
                var dateTimeValue = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                MegaObject.SetPropertyValue(property.Id, dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            else
            {
                MegaObject.SetPropertyValue(property.Id, value, format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);
            }
        }

        internal async Task UpdateElement(IEnumerable<ISetter> setters)
        {
            if(setters == null)
            {
                return;
            }

            foreach(ISetter setter in setters)
            {
                //_domainModel.LogInformation("before setter");
                if(setter is PropertySetter ps)
                {
                    //_domainModel.LogInformation($"setter prop = {ps.PropertyDescription}");
                    SetValue(ps.PropertyDescription, ps.Value, ps.SetterFormat);
                }
                else if(setter is CollectionSetter cs)
                {
                    //_domainModel.LogInformation($"setter col = {cs.ToString()}");
                    var link = cs.RelationshipDescription;
                    var permissions = GetPathCrud(link.Path[0]);
                    switch(cs.Action)
                    {
                        case CollectionAction.ReplaceAll:
                            if (!permissions.IsCreatable || !permissions.IsDeletable)
                            {
                                throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                            }
                            RemoveAllConnection(link.Path);
                            foreach(var element in cs.Elements)
                            {
                                await ConnectToAsync(link, element.Id, true);
                            }
                            break;
                        case CollectionAction.Add:
                            if (!permissions.IsCreatable)
                            {
                                throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                            }
                            foreach(var element in cs.Elements)
                            {
                                await ConnectToAsync(link, element.Id, true);
                            }
                            break;
                        case CollectionAction.Remove:
                            if (!permissions.IsDeletable)
                            {
                                throw new ExecutionError($"You are not allowed to perform this action on this property ({link.Path[0].RoleName})");
                            }
                            foreach(var element in cs.Elements)
                            {
                                await ConnectToAsync(link, element.Id, false);
                            }
                            break;
                    }
                }
                //_domainModel.LogInformation("after setter");
            }
        }

        private async Task<IModelElement> ConnectToAsync(IRelationshipDescription link, string id, bool insert)
        {
            var elemSchema = ClassDescription.MetaModel.GetClassDescription(link.Path.Last().TargetSchemaName);

            var getElementByIdAsyncTask = _domainModel.GetElementByIdAsync(elemSchema, id);
            if(getElementByIdAsyncTask == null)
            {
                throw new Exception($"Element {elemSchema.Name} not found with id {id}");
            }

            if(!(await getElementByIdAsyncTask is HopexModelElement elemToInsert))
            {
                throw new Exception($"{id}' is not a valid item to add for relationship {link.Name} of {id}");
            }

            var current = insert ? InsertConnection(elemToInsert, link.Path) : RemoveConnection(elemToInsert, link.Path);

            return current != null ? new HopexModelElement(_domainModel, elemSchema, current, id) : null;
        }

        private MegaObject InsertConnection(HopexModelElement elemToInsert, IPathDescription [] path)
        {
            var current = MegaObject;
            var interObjects = new List<MegaObject>();
            var found = FillInterObjects(current, elemToInsert.MegaObject, path, ref interObjects);
            if(found) //adding a new connection from a source to a target already linked together is forbidden
            {
                return current;
            }

            for(int ix = 0;ix < path.Length;ix++)
            {
                bool isLast = ix == path.Length - 1;
                IPathDescription hop = path [ix];

                MegaId roleId = hop.RoleId;

                var collection = current.GetCollection(roleId);
                if(isLast)
                {
                    collection.Add(Utils.NormalizeHopexId(elemToInsert.Id));
                    current = elemToInsert.MegaObject;
                }
                else
                {
                    current = collection.Create();
                }
                CreateCondition(current, hop.Condition);
            }
            return current;
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

        private MegaObject RemoveConnection(HopexModelElement elemToInsert, IPathDescription [] path)
        {
            var current = MegaObject;
            List<MegaObject> interObjects = new List<MegaObject>();
            var found = FillInterObjects(current, elemToInsert.MegaObject, path, ref interObjects);
            if(!found) //Inexisting link
            {
                return current;
            }

            if(interObjects.Any()) //remove interObject will automatically kill links
            {
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if(path.Any()) //remove single link
            {
                IPathDescription hop = path.First();
                MegaId roleId = hop.RoleId;
                var collection = current.GetCollection(roleId);
                var id = Utils.NormalizeHopexId(elemToInsert.Id);
                RemoveFromCollection(collection, id);
            }
            return current;
        }

        private void RemoveAllConnection(IPathDescription [] paths)
        {
            bool hasInterObjects = paths.Length > 1;
            if(hasInterObjects)
            {
                List<MegaObject> interObjects = new List<MegaObject>();
                FillInterObjects(MegaObject, null, paths, ref interObjects);
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if(paths.Length == 1)
            {
                RemoveAllLinks(MegaObject, paths [0]);
            }
        }

        bool FillInterObjects(MegaObject source, MegaObject target, IPathDescription [] paths, ref List<MegaObject> interObjects, int idx = 0)
        {
            bool isLast = idx == paths.Length - 1;
            MegaId roleId = paths [idx].RoleId;
            var collection = source.GetCollection(roleId);
            var enumerator = collection.GetEnumerator();
            if((target == null) && isLast)
            {
                return true;
            }

            while(enumerator.MoveNext())
            {
                var item = (MegaObject)enumerator.Current;
                if(isLast) //for last path, we find target id
                {
                    if(item.IsSameId(target.Id) && MetaAssociationConditionFilter(item, paths[idx]))
                    {
                        return true;
                    }
                }
                else //for previous paths, we find interobjects
                {
                    if(FillInterObjects(item, target, paths, ref interObjects, idx + 1))
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

        private static bool MetaAssociationConditionFilter(MegaObject item, IPathDescription hop)
        {
            if(hop.Condition == null || string.IsNullOrEmpty(hop.Condition.RoleId) || string.IsNullOrEmpty(hop.Condition.ObjectFilterId))
            {
                return true;
            }
            return item.GetPropertyValue(Utils.NormalizeHopexId(hop.Condition.RoleId)) == hop.Condition.ObjectFilterId;
        }

        void RemoveAllLinks(MegaObject source, IPathDescription path)
        {
            MegaId roleId = path.RoleId;
            var collection = source.GetCollection(roleId);
            var enumerator = collection.GetEnumerator();
            var ids = new List<MegaId>();
            while(enumerator.MoveNext())
            {
                var item = (MegaObject)enumerator.Current;
                ids.Add(item.Id);
            }
            ids.ForEach(id => RemoveFromCollection(collection, id));
        }

        public void Dispose()
        {
            _domainModel = null;
            (MegaObject as IDisposable)?.Dispose();
        }

        private void RemoveFromCollection(MegaCollection collection, MegaObject item)
        {
            RemoveFromCollection(collection, item.Id);
        }

        private void RemoveFromCollection(MegaCollection collection, MegaId id)
        {
            collection.NativeObject.Item(id.Value).Remove();
        }

        public CrudResult GetCrud()
        {
            return CrudComputer.GetCrud(MegaObject);
        }

        public CrudResult GetPropertyCrud(IPropertyDescription property)
        {
            return CrudComputer.GetPropertyCrud(MegaObject, property);
        }

        public CrudResult GetPathCrud(IPathDescription path)
        {
            return CrudComputer.GetPathCrud(MegaObject, path);
        }

        public bool IsConfidential => MegaObject.NativeObject.IsConfidential;

        public bool IsAvailable => MegaObject.NativeObject.CallFunction("IsAvailable");
    }
}
