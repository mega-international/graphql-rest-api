using GraphQL.Execution;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hopex.Model.Mocks
{
    internal class MockDataElement : IModelElement
    {
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();

        public MegaObject MegaObject => new MegaObject();

        public IMegaObject IMegaObject => throw new NotImplementedException();

        public IMegaObject Parent => throw new NotImplementedException();
        public IHopexDataModel DomainModel { get; private set; }

        private readonly List<Exception> _errors = new List<Exception>();
        public IEnumerable<Exception> Errors { get => _errors; }

        public MockDataElement(MockDataModel dataModel, IClassDescription metaclass, string id = null, string name = null)
        {
            DataModel = dataModel;
            ClassDescription = metaclass;
            Id = id ?? dataModel.NextId(metaclass.Name);
            var prop = metaclass.GetPropertyDescription("Name", false);
            if (prop != null)
            {
                SetValue(prop, name ?? DataGenerator.CreateRandom(PropertyType.String) as string);
            }
        }

        public MegaId Id { get; }
        public MockDataModel DataModel { get; }

        public IClassDescription ClassDescription { get; }
        public IHopexMetaModel MetaModel => ClassDescription.MetaModel;

        public Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            return GetCollectionAsync(name, getCollectionArguments.Erql, getCollectionArguments.OrderByClauses, relationshipName, getCollectionArguments.AdHocPredicate);
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string erql, List<Tuple<string, int>> orderByClauses = null, string relationshipName = null, Func<IModelElement, bool> adHocPredicate = null)
        {
            IRelationshipDescription relationshipDescription = ClassDescription.GetRelationshipDescription(name, false) ?? ClassDescription.GetRelationshipDescription(relationshipName);
            return Task.FromResult((IModelCollection)_values.GetOrAdd(relationshipDescription.Name, _ => DataGenerator.CreateCollection(DataModel, relationshipDescription)));
        }

        public T GetValue<T>(IPropertyDescription propertyDescription, IDictionary<string, ArgumentValue> arguments = null, string format = null)
        {
            return (T)_values.GetOrAdd(propertyDescription.Name, _ => DataGenerator.CreateRandom(propertyDescription));
        }

        public void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null)
        {
            _values.AddOrUpdate(propertyDescription.Name, value, (_, v) => value);
        }

        public CrudResult GetCrud()
        {
            return new CrudResult("CRUD");
        }

        public CrudResult GetPropertyCrud(IPropertyDescription property)
        {
            return new CrudResult("CRUD");
        }

        public object GetGenericValue(string propertyMegaId, IDictionary<string, ArgumentValue> arguments)
        {
            throw new NotImplementedException();
        }

        public IModelCollection GetGenericCollection(string collectionMegaId)
        {
            throw new NotImplementedException();
        }

        public void AddErrors(IModelElement subElement)
        {
            foreach (var error in subElement.Errors)
            {
                _errors.Add(error);
            }
        }

        public void CreateContext(IModelElement targetLinkAttributes, IEnumerable<IPropertyDescription> linkAttributes)
        {
            throw new NotImplementedException();
        }

        public void SpreadContextFromParent()
        {
            throw new NotImplementedException();
        }

        public IModelElement BuildChildElement(IMegaObject megaObject, IRelationshipDescription relationship, int pathIdx)
        {
            if(relationship == null)
            {
                throw new NullReferenceException("relationship is null");
            }
            return DataModel.BuildElement(megaObject, relationship.Path[pathIdx].TargetClass);
        }

        public Task<IModelElement> GetElementByIdAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType)
        {
            return DataModel.GetElementByIdAsync(relationship.TargetClass, id, idType);
        }

        public Task<IModelElement> LinkElementAsync(IRelationshipDescription relationship, bool useInstanceCreator, IModelElement elementToLink, IEnumerable<ISetter> setters)
        {
            throw new NotImplementedException();
        }

        public Task<IModelElement> CreateElementAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
        {
            return DataModel.CreateElementAsync(relationship.TargetClass, id, idType, useInstanceCreator, setters);
        }

        public async Task<IModelElement> UpdateAsync(IEnumerable<ISetter> setters)
        {
            if(setters == null)
            {
                return this;
            }

            foreach(ISetter setter in setters)
            {
                if(setter is PropertySetter ps)
                {
                    await setter.UpdateElementAsync(DataModel, this);
                }
                else if(setter is CollectionSetter cs)
                {
                    IRelationshipDescription link = cs.RelationshipDescription;
                    var collection = await GetCollectionAsync(link.Name, null) as MockModelCollection;
                    var elements = cs.ListElement.Cast<Dictionary<string, object>>();

                    switch(cs.Action)
                    {
                        case CollectionAction.ReplaceAll:
                            collection.Clear();
                            goto case CollectionAction.Add;

                        case CollectionAction.Add:
                            foreach(var element in elements)
                            {
                                var e = await GetElementByIdAsync(link, element ["id"].ToString(), IdTypeEnum.INTERNAL);
                                if(e == null)
                                {
                                    throw new Exception($"Invalid {element ["id"]} when adding a new relationship");
                                }
                                collection.Add(e);
                            }
                            break;
                        case CollectionAction.Remove:
                            foreach(var element in elements)
                            {
                                var e = await GetElementByIdAsync(link, element ["id"].ToString(), IdTypeEnum.INTERNAL);
                                collection.Remove(e.Id, true);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            return this;
        }

        public bool IsReadOnly(IPropertyDescription property)
        {
            return false;
        }

        public bool IsReadWrite(IPropertyDescription property)
        {
            return true;
        }

        public bool IsConfidential => false;
        public bool IsAvailable => true;
        public IMegaObject Language { get; set; }
        public IModelContext Context => throw new NotImplementedException();
        IModelElement IModelElement.Parent => throw new NotImplementedException();
    }
}
