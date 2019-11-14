using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.DataModel
{
    internal class HopexModelElement : IModelElement, IDisposable
    {
        private HopexDataModel _domainModel;
        private MegaObject _nativeObject;

        public HopexModelElement(HopexDataModel domainModel, IClassDescription schema, MegaObject nativeObject, MegaId id = null)
        {
            _domainModel = domainModel;
            ClassDescription = schema;
            _nativeObject = nativeObject;
            Id = id;
            if(id == null)
            {
                id = Utils.SanitizeId(nativeObject.MegaField);
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

        public Task<IModelCollection> GetCollectionAsync(string name)
        {
            IRelationshipDescription relationshipDescription = ClassDescription.GetRelationshipDescription(name);
            return Task.FromResult<IModelCollection>(new HopexModelCollection(_domainModel, relationshipDescription, _nativeObject));
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
            switch(property.PropertyType)
            {
                case PropertyType.String:
                case PropertyType.RichText:
                    if(string.Equals(property.Name, "name", StringComparison.OrdinalIgnoreCase))
                    {
                        return _nativeObject.GetPropertyValue<T>(GetNameProperty(property.Id), format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat);
                    }
                    if(format != null)
                    {
                        if(format == "RAW")
                        {
                            format = "ANSI";
                        }
                        return _nativeObject.NativeObject.GetFormated(property.Id.ToString(), format);
                    }
                    return _nativeObject.GetPropertyValue<T>(property.Id, property.GetterFormat ?? PropertyDescription.DefaultGetterFormat);
                case PropertyType.Enum:
                    var enumResult = _nativeObject.GetPropertyValue<T>(property.Id, format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat);
                    var q = from x in property.EnumValues
                            where x.InternalValue == enumResult.ToString()
                            select x.Name;
                    return (T)Convert.ChangeType(q.FirstOrDefault(), typeof(T));
                case PropertyType.Date:
                    var dateResult = _nativeObject.GetPropertyValue<T>(property.Id, format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat);
                    if(dateResult.ToString() == "")
                    {
                        return (T)Convert.ChangeType(null, typeof(T));
                    }
                    return dateResult;
                default:
                    return _nativeObject.GetPropertyValue<T>(property.Id, format ?? property.GetterFormat ?? PropertyDescription.DefaultGetterFormat);
            }
        }

        public void SetValue<T>(IPropertyDescription property, T value, string format = null)
        {
            if(string.Equals(property.Name, "name", StringComparison.OrdinalIgnoreCase))
            {
                _nativeObject.SetPropertyValue(GetNameProperty(property.Id), value, format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);
            }
            else if(value is DateTime)
            {
                var dateTimeValue = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                _nativeObject.SetPropertyValue(property.Id, dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            else
            {
                _nativeObject.SetPropertyValue(property.Id, value, format ?? property.SetterFormat ?? PropertyDescription.DefaultSetterFormat);
            }
        }

        private MegaId GetNameProperty(MegaId propertyId)
        {
            var root = _nativeObject.NativeObject.GetRoot;
            var shortNameProperty = root.GetObjectFromid("~Z20000000D60[Short Name]");
            var classDescription = _nativeObject.NativeObject.GetClassObject();
            var mainProperties = classDescription.Description.item(1).MainProperties;
            return mainProperties(shortNameProperty).Exists ? "~Z20000000D60[Short Name]" : propertyId;
        }

        internal async Task UpdateElement(IEnumerable<ISetter> setters)
        {
            if(setters == null)
            {
                return;
            }

            foreach(ISetter setter in setters)
            {
                if(setter is PropertySetter ps)
                {
                    SetValue(ps.PropertyDescription, ps.Value, ps.SetterFormat);
                }
                else if(setter is CollectionSetter cs)
                {
                    var link = cs.RelationshipDescription;
                    switch(cs.Action)
                    {
                        case CollectionAction.ReplaceAll:
                            RemoveAllConnection(link.Path);
                            foreach(var id in cs.Ids)
                            {
                                await ConnectToAsync(link, id, true);
                            }
                            break;

                        case CollectionAction.Add:
                            foreach(var id in cs.Ids)
                            {
                                await ConnectToAsync(link, id, true);
                            }
                            break;
                        case CollectionAction.Remove:
                            foreach(var id in cs.Ids)
                            {
                                await ConnectToAsync(link, id, false);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private async Task<IModelElement> ConnectToAsync(IRelationshipDescription link, string id, bool insert)
        {
            IClassDescription elemSchema = ClassDescription.MetaModel.GetClassDescription(link.Path.Last().TargetSchemaName);
            var elemToInsert = ((await _domainModel.GetElementByIdAsync(elemSchema, id)) as HopexModelElement);
            if(elemToInsert == null)
            {
                throw new Exception($"{id}' is not a valid item to add for relationship {link.Name} of {id}");
            }

            var current = insert ? InsertConnection(elemToInsert, link.Path) : RemoveConnection(elemToInsert, link.Path);
            return current != null ? new HopexModelElement(_domainModel, elemSchema, current, id) : null;
        }

        private MegaObject InsertConnection(HopexModelElement elemToInsert, IPathDescription [] path)
        {
            var current = _nativeObject;
            List<MegaObject> interObjects = null;
            var found = FillInterObjects(current, elemToInsert._nativeObject, path, ref interObjects);
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
                    current = elemToInsert._nativeObject;
                }
                else
                {
                    current = collection.Create();
                }
            }
            return current;
        }

        private MegaObject RemoveConnection(HopexModelElement elemToInsert, IPathDescription [] path)
        {
            var current = _nativeObject;
            List<MegaObject> interObjects = new List<MegaObject>();
            var found = FillInterObjects(current, elemToInsert._nativeObject, path, ref interObjects);
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
                FillInterObjects(_nativeObject, null, paths, ref interObjects);
                interObjects.ForEach(interObject => interObject.Delete("NoHierarchy"));
            }
            else if(paths.Length == 1)
            {
                RemoveAllLinks(_nativeObject, paths [0]);
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
                    if(item.IsSameId(target.Id))
                    {
                        return true;
                    }
                }
                else //for previous paths, we find interobjects
                {
                    if(FillInterObjects(item, target, paths, ref interObjects, idx + 1))
                    {
                        interObjects.Add(item);
                        if(target != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return (target == null); //target null is always true
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
                ids.Add(Utils.NormalizeHopexId(item.Id));
            }
            ids.ForEach(id => RemoveFromCollection(collection, id));
        }

        public void Dispose()
        {
            _domainModel = null;
            (_nativeObject as IDisposable)?.Dispose();
        }

        private void RemoveFromCollection(MegaCollection collection, MegaObject item)
        {
            RemoveFromCollection(collection, item.Id);
        }

        private void RemoveFromCollection(MegaCollection collection, MegaId id)
        {
            collection.NativeObject.Item(id?.ToString()).Remove();
        }
    }
}
