using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.Mocks
{

    public class MockDataModel : IHopexDataModel, IDisposable
    {
        private int cx = 1000;
        private Dictionary<string, int> _counters = new Dictionary<string, int>();
        private ConcurrentDictionary<MegaId, IModelElement> _elements = new ConcurrentDictionary<MegaId, IModelElement>();
        private ConcurrentDictionary<string, MockModelCollection> _collections = new ConcurrentDictionary<string, MockModelCollection>();

        public static IHopexDataModel Create(IHopexMetaModel metaModel)
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

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            var coll = await GetCollectionAsync(schema.Name, null) as MockModelCollection;
            var elem = CreateElement(schema, cx++);
            coll.Add(elem);
            await UpdateAsync(elem, setters);
            return elem;
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
                    elem.SetValue(ps.PropertyDescription, ps.Value, ps.SetterFormat);
                }
                else if (setter is CollectionSetter cs)
                {
                    IRelationshipDescription link = cs.RelationshipDescription;
                    var collection = await elem.GetCollectionAsync(link.Name, null) as MockModelCollection;
                    var targeSchema = MetaModel.GetClassDescription(link.Path.Last().TargetSchemaName);
                    switch (cs.Action)
                    {
                        case CollectionAction.ReplaceAll:
                            collection.Clear();
                            goto case CollectionAction.Add;

                        case CollectionAction.Add:
                            foreach (var element in cs.Elements)
                            {
                                var e = await GetElementByIdAsync(targeSchema, element.Id);
                                if (e == null)
                                {
                                    throw new Exception($"Invalid {element.Id} when adding a new relationship");
                                }
                                collection.Add(e);
                            }
                            break;
                        case CollectionAction.Remove:
                            foreach (var element in cs.Elements)
                            {
                                var e = await GetElementByIdAsync(targeSchema, element.Id);
                                collection.Remove(e.Id, true);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string erql, List<Tuple<string, int>> orderByClauses = null, string relationshipName = null)
        {
            var schema = MetaModel.GetClassDescription(name);
            return Task.FromResult<IModelCollection>(_collections.GetOrAdd(schema.Name, _ => new MockModelCollection(this, schema)));
        }

        public Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id)
        {
            if (_elements.TryGetValue(id, out var elem))
                return Task.FromResult(elem);
            return Task.FromResult<IModelElement>(null);
        }

        public Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, bool cascade)
        {
            if (_collections.TryGetValue(schema.Name, out var collections))
            {
                if (collections.Remove(id, cascade))
                {
                    // return Task.FromResult(true);
                }
            }
            return Task.FromResult<IModelElement>(null);
        }

        internal IModelElement CreateElement(IClassDescription targetSchema, int id)
        {
            var elem = new MockDataElement(this, targetSchema, $"{targetSchema.Name.ToLower()}-{id}");
            _elements.TryAdd(elem.Id, elem);
            return elem;
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IEnumerable<ISetter> setters)
        {
            var elem = await GetElementByIdAsync(schema, id);
            if (elem == null) throw new Exception("Not found");
            await UpdateAsync(elem, setters);
            return elem;
        }

        public void Dispose()
        {
            // do nothing for making asserts
        }
    }
}
