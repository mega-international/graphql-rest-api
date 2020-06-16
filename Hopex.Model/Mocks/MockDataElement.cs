using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;

namespace Hopex.Model.Mocks
{
    internal class MockDataElement : IModelElement
    {
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();

        public MegaObject MegaObject => new MegaObject();

        public IMegaObject IMegaObject => throw new NotImplementedException();

        public IMegaObject Parent => throw new NotImplementedException();
        public IHopexDataModel DomainModel { get; private set; }

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

        public T GetValue<T>(IPropertyDescription propertyDescription, Dictionary<string, object> arguments = null, string format = null)
        {
            return (T)_values.GetOrAdd(propertyDescription.Name, _ => DataGenerator.CreateRandom(propertyDescription));
        }

        public void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null)
        {
            _values.AddOrUpdate(propertyDescription.Name, value, (_, v) => value);
        }

        public T GetValue<T>(string propertyName, Dictionary<string, object> arguments = null, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            return GetValue<T>(property, arguments, format);
        }

        public void SetValue<T>(string propertyName, T value, string format = null)
        {
            var property = ClassDescription.GetPropertyDescription(propertyName);
            SetValue<T>(property, value, format);
        }

        public CrudResult GetCrud()
        {
            return new CrudResult("CRUD");
        }

        public object GetGenericValue(string popertyMegaId, Dictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        public IModelCollection GetGenericCollection(string collectionMegaId)
        {
            throw new NotImplementedException();
        }

        public bool IsConfidential => false;

        public bool IsAvailable => true;
    }
}
