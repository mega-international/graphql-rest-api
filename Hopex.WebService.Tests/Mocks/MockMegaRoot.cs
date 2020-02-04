using Hopex.Model.Mocks;
using Hopex.WebService.Tests.Mocks.Drawings;

using Mega.Macro.API;

using Moq;

using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaRoot : MockMegaObject, IMegaRoot, ISupportsDiagnostics
    {
        private Dictionary<MegaId, MockMegaObject> _objects = new Dictionary<MegaId, MockMegaObject>(new MegaIdComparer());
        private Dictionary<MegaId, MockCollectionDescription> _collectionDescriptions = new Dictionary<MegaId, MockCollectionDescription>(new MegaIdComparer());

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

            internal MockMegaRoot Build()
            {
                return _root;
            }
        }

        public virtual IMegaObject GetObjectFromId(MegaId objectId)
        {
            return _objects[objectId];
        }

        public virtual object CallFunction(string function, params object[] parameters)
        {
            return "";
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
    }
}
