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
using GraphQL.Types;
using GraphQL;

namespace Hopex.Model.Mocks
{

    public class MockDataModel : IHopexDataModel, IDisposable
    {
        private int cx = 1000;
        private readonly Dictionary<string, int> _counters = new Dictionary<string, int>();
        private readonly ConcurrentDictionary<MegaId, IModelElement> _elements = new ConcurrentDictionary<MegaId, IModelElement>();
        private readonly ConcurrentDictionary<string, MockModelCollection> _collections = new ConcurrentDictionary<string, MockModelCollection>();

        public Dictionary<string, IModelElement> TemporaryMegaObjects { get; }
        
        public static MockDataModel Create(IHopexMetaModel metaModel)
        {
            MockDataModel dataModel = new MockDataModel(metaModel);
            var idx = 0;
            foreach (IClassDescription metaclass in metaModel.Classes)
            {
                dataModel.CreateElement(metaclass, idx);
            }
            return dataModel;
        }

        internal string NextId(string name)
        {
            name = name.ToLower();
            if (!_counters.TryGetValue(name, out int cx))
            {
                cx = 1;
            }
            else
            {
                cx++;
            }
            _counters[name] = cx;
            return $"{name}-{cx}";
        }

        private MockDataModel(IHopexMetaModel metaModel)
        {
            MetaModel = metaModel;
        }

        public IHopexMetaModel MetaModel { get; }
        public static int MaxCollectionSize { get; set; } = 4;

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
        {
            var coll = await GetCollectionAsync(schema.Name) as MockModelCollection;
            var elem = CreateElement(schema, cx++);
            coll.Add(elem);
            await UpdateAsync(elem, setters);
            return elem;
        }

        public async Task<IModelElement> CreateElementFromParentAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters, IMegaCollection iColParent)
        {
            return await CreateElementAsync(schema, id, idType, useInstanceCreator, setters);
        }

        private async Task UpdateAsync(IModelElement elem, IEnumerable<ISetter> setters)
        {
            if (setters == null)
            {
                return;
            }

            foreach (ISetter setter in setters)
            {
                if (setter is PropertySetter ps)
                {
                    await setter.UpdateElementAsync(this, elem);
                }
                else if (setter is CollectionSetter cs)
                {
                    IRelationshipDescription link = cs.RelationshipDescription;
                    var collection = await ((MockDataElement)elem).GetCollectionAsync(link.Name, null) as MockModelCollection;
                    var targeSchema = MetaModel.GetClassDescription(link.Path.Last().TargetSchemaName);

                    var elements = cs.ListElement.Cast<Dictionary<string, object>>();

                    switch (cs.Action)
                    {
                        case CollectionAction.ReplaceAll:
                            collection.Clear();
                            goto case CollectionAction.Add;

                        case CollectionAction.Add:
                            foreach (var element in elements)
                            {
                                var e = await GetElementByIdAsync(targeSchema, element["id"].ToString(), IdTypeEnum.INTERNAL);
                                if (e == null)
                                {
                                    throw new Exception($"Invalid {element["id"]} when adding a new relationship");
                                }
                                collection.Add(e);
                            }
                            break;
                        case CollectionAction.Remove:
                            foreach (var element in elements)
                            {
                                var e = await GetElementByIdAsync(targeSchema, element["id"].ToString(), IdTypeEnum.INTERNAL);
                                collection.Remove(e.Id, true);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public async Task<IModelElement> CreateUpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            var coll = await GetCollectionAsync(schema.Name) as MockModelCollection;
            var elem = CreateElement(schema, cx++);
            coll.Add(elem);
            await UpdateAsync(elem, setters);
            return elem;
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            return GetCollectionAsync(name);
        }

        public Task<IModelCollection> GetCollectionAsync(string name)
        {
            var schema = MetaModel.GetClassDescription(name);
            return Task.FromResult<IModelCollection>(_collections.GetOrAdd(schema.Name, _ => new MockModelCollection(this, schema)));
        }

        public Task<List<IModelElement>> SearchAllAsync(IResolveFieldContext<IHopexDataModel> ctx)
        {
            var collection = _elements.Select(modelElement => modelElement.Value).ToList();
            return Task.FromResult(collection);
        }

        public Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id, IdTypeEnum idType)
        {
            if (_elements.TryGetValue(id, out var elem))
                return Task.FromResult(elem);
            return Task.FromResult<IModelElement>(null);
        }

        public Task<DeleteResultType> RemoveElementAsync(List<IMegaObject> objectsToDelete, bool isCascade = false)
        {
            var removedElementCount = 0;
            foreach (var item in objectsToDelete)
            {
                if (_collections.TryGetValue("", out var collections))
                {
                    if (collections.Remove(item.Id, isCascade))
                    {
                        removedElementCount++;
                    }
                }
            }
            return Task.FromResult(new DeleteResultType {DeletedCount = removedElementCount});
        }

        internal IModelElement CreateElement(IClassDescription targetSchema, int id)
        {
            var elem = new MockDataElement(this, targetSchema, $"{targetSchema.Name.ToLower()}-{id}");
            _elements.TryAdd(elem.Id, elem);
            return elem;
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters)
        {
            var elem = await GetElementByIdAsync(schema, id, IdTypeEnum.INTERNAL);
            if (elem == null) throw new Exception("Not found");
            return await UpdateElementAsync(elem, setters);
        }

        public async Task<IModelElement> UpdateElementAsync(IModelElement element, IEnumerable<ISetter> setters)
        {
            await UpdateAsync(element, setters);
            return element;
        }

        public void Dispose()
        {
            // do nothing for making asserts
        }
    }
}
