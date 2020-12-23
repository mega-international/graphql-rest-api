using Hopex.Model.Abstractions;
using Mega.Macro.API;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaCollection : MockMegaItem, IMegaCollection
    {
        public MegaId Id { get; private set; }
        private List<MockMegaObject> _children = new List<MockMegaObject>();

        public MockMegaCollection(MegaId id)
        {
            Id = id;
        }

        internal MockMegaCollection WithChildren(MockMegaObject o)
        {
            _children.Add(o);
            return this;
        }

        public IMegaObject Item(int index)
        {
            return _children[index - 1];
        }

        public IMegaObject Item(MegaId objectId)
        {
            var comparer = new MegaIdComparer();
            return _children.Find(o => comparer.Equals(o.Id, objectId));
        }

        public IEnumerator<IMegaObject> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void PopagateRoot(MockMegaRoot root, MockMegaRoot.Builder builder)
        {
            Root = root;
            foreach (var child in _children)
            {
                child.PropagateRoot(root, builder);
                builder.WithObject(child);
            }
        }

        public IMegaObject Create(MegaId objectId = null, string paramId1 = null, string paramValue1 = null, string paramId2 = null, string paramValue2 = null)
        {
            var child = new MockMegaObject(objectId, Id);            
            _children.Add(child);
            child.Root = Root;
            ((MockMegaRoot)Root).AddMetaLegs(child);
            return child;
        }

        public IMegaObject Add(MegaId objectId, MegaId propertyId = null)
        {
            MegaIdUtils.EnsureValidPropertyId(objectId, "MegaCollection.Add"); // Do not necessarily throw an error in real life but do strange things
            var child = Root.GetObjectFromId(objectId);
            _children.Add((MockMegaObject)child);
            return child;
        }

        public void RemoveChild(MegaId id)
        {
            var comparer = new MegaIdComparer();
            _children.RemoveAll(o => comparer.Equals(o.Id, id));
        }

        public override T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            /*var idComparer = new MegaIdComparer();
            if(idComparer.Equals(methodId, "~GuX91iYt3z70[InstanceCreator]"))
            {
                IMegaWizardContext wizard = new MockMegaWizardContext(this);
                return (T)(wizard);
            }*/
            return base.CallFunction<T>(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public IMegaCollection GetType(string targetMetaClassId)
        {
            return this;
        }
    }
}
