using Hopex.Model.Abstractions;
using Hopex.WebService.Tests.Mocks.Drawings;

using Mega.Macro.API;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaRoot : MockMegaObject, IMegaRoot, ISupportsDiagnostics
    {
        private Dictionary<MegaId, MockMegaObject> _objects = new Dictionary<MegaId, MockMegaObject>(new MegaIdComparer());
        private Dictionary<MegaId, MockClassDescription> _classDescriptions = new Dictionary<MegaId, MockClassDescription>(new MegaIdComparer());
        private Dictionary<MegaId, MockCollectionDescription> _collectionDescriptions = new Dictionary<MegaId, MockCollectionDescription>(new MegaIdComparer());
        private IDictionary<MegaId, List<MegaId>> _metaLegs = new Dictionary<MegaId, List<MegaId>>(new MegaIdComparer());

        public IMegaCurrentEnvironment CurrentEnvironment => new MockCurrentEnvironment();

        private List<string> _generatedERQLS = new List<string>();
        

        public void AddGeneratedERQL(string erql)
        {
            _generatedERQLS.Add(erql.TrimEnd(' '));
        }

        public IEnumerable<string> GeneratedERQLs => _generatedERQLS;

        public class Builder
        {
            MockMegaRoot _root;

            public Builder()
            {
                _root = new MockMegaRoot();
            }

            public Builder(Mock<MockMegaRoot> spyRoot)
            {
                _root = spyRoot.Object;
            }

            public Builder WithObject(MockMegaObject o)
            {
                o.Root = _root;
                _root._objects.Add(o.Id, o);
                return this;
            }

            public Builder WithRelation(MockMegaCollection col)
            {
                col.Root = _root;
                _root.WithRelation(col);
                return this;
            }

            internal Builder WithClassDescription(MockClassDescription description)
            {
                _root._classDescriptions[description.Id] = description;
                return this;
            }

            internal Builder WithCollectionDescription(MockCollectionDescription description)
            {
                _root._collectionDescriptions[description.Id] = description;
                return this;
            }
            
            internal Builder WithMetaAssociationEnd(MegaId metaclassId, MegaId legId)
            {
                if (! _root._metaLegs.ContainsKey(metaclassId))
                    _root._metaLegs[metaclassId] = new List<MegaId>();
                _root._metaLegs[metaclassId].Add(legId);
                return this;
            }

            internal MockMegaRoot Build()
            {
                _root.Root = _root;
                CreateMetaclassCollections();
                PropagateRoot();
                return _root;
            }

            private void CreateMetaclassCollections()
            {
                var objectsByClassId = _root._objects
                                    .GroupBy(p => p.Value.GetClassId())
                                    .Where(g => g.Key != null);
                foreach (var classGroup in objectsByClassId)
                {
                    var collection = new MockMegaCollection(classGroup.Key);
                    collection.Root = _root;
                    foreach (var pair in classGroup)
                    {
                        collection.WithChildren(pair.Value);
                    }
                    _root.WithRelation(collection);
                }
            }

            private void PropagateRoot()
            {
                var objectListCopy = _root._objects.ToList();
                foreach (var pair in objectListCopy)
                {
                    pair.Value.PropagateRoot(_root, this);
                }                
            }
        }

        internal void AddMetaLegs(MockMegaObject child)
        {
            if (_metaLegs.ContainsKey(child.GetClassId()))
            {
                foreach (var legId in _metaLegs[child.GetClassId()])
                {
                    var col = new MockMegaCollection(legId);
                    col.Root = this;
                    child.WithRelation(col);
                }                    
            }
        }

        public virtual IMegaObject GetObjectFromId(MegaId objectId)
        {
            if (_objects.TryGetValue(objectId, out var iObject))
                return iObject;
            return new InexistingMockMegaObject();
        }

        public virtual object CallFunction(string function, params object[] parameters)
        {
            return "";
        }

        public virtual IMegaObject GetClassDescription(MegaId classId)
        {
            return _classDescriptions[classId];
        }

        public virtual IMegaObject GetCollectionDescription(MegaId collectionId)
        {
            if (!_collectionDescriptions.ContainsKey(collectionId))
                _collectionDescriptions.Add(collectionId, new MockCollectionDescription(collectionId));
            return _collectionDescriptions[collectionId];
        }

        public IMegaDrawingFactory GetDrawingFactory()
        {
            return new MockMegaDrawingFactory();
        }

        public IMegaCollection GetSelection(string query, int sortDirection1 = 1, string sortCriterion1 = null, int sortDirection2 = 1, string sortCriterion2 = null, int sortDirection3 = 1, string sortCriterion3 = null)
        {
            if (IsGetObjectFromId(query, out var idMetaclass, out var idObject))
            {
                var iObj = GetObjectFromId(idObject);
                var col = new MockMegaCollection(idMetaclass);
                col.Root = Root;
                return col.WithChildren((MockMegaObject)iObj);
            }                
            throw new NotImplementedException($"Query {query}");
        }

        private bool IsGetObjectFromId(string query, out MegaId idMetaclass, out MegaId idObject)
        {
            idMetaclass = null;
            idObject = null;
            var pattern = @"SELECT (~[\w\d\(\)]{12}(\[.*\])?) WHERE ~310000000D00\[Absolute Identifier\] = \""(~?[\w\d\(\)]{12}(\[.*\])?)\""";
            var match = Regex.Match(query, pattern);
            if (match.Success)
            {
                idMetaclass = MegaId.Create(match.Groups[1].Value);
                idObject = MegaId.Create(match.Groups[3].Value);
                return true;
            }
            return false;
        }
    }
}
