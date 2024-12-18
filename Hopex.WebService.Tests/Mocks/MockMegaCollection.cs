using Hopex.Model.Abstractions;
using Mega.Macro.API;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaCollection : MockMegaItem, IMegaCollection
    {
        public MegaId Id { get; private set; }
        public MockMegaObject Source { get; set; } //megaobject d'origine de la collection, son parent

        private readonly List<MockMegaObject> _children = new List<MockMegaObject>();
        private static int _idGenerator = 1;

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
            return _children[index - 1].FromCollection(this);
        }

        public IMegaObject Item(MegaId objectId)
        {
            var comparer = new MegaIdComparer();
            return _children.Find(o => comparer.Equals(o.Id, objectId)).FromCollection(this);
        }

        public IEnumerator<IMegaObject> GetEnumerator()
        {
            foreach(var child in _children)
            {
                yield return child.FromCollection(this);
            }
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
            if(objectId == null)
            {
                objectId = GenerateId();
            }
            var child = new MockMegaObject(objectId, Id);
            _children.Add(child);
            child.Root = Root;
            var mockRoot = (MockMegaRoot)Root;
            mockRoot.AddMetaLegs(child);
            mockRoot.AddNewObject(child);
            return child.FromCollection(this);
        }

        public IMegaObject Add(MegaId objectId, MegaId propertyId = null)
        {
            MegaIdUtils.EnsureValidPropertyId(objectId, "MegaCollection.Add"); // Do not necessarily throw an error in real life but do strange things
            var child = (MockMegaObject)Root.GetObjectFromId(objectId);
            _children.Add(child);
            return child.FromCollection(this);
        }

        public void RemoveChild(MegaId id)
        {
            var comparer = new MegaIdComparer();
            _children.ForEach(mo => mo.Collection = null);
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

        public static MegaId GenerateId()
        {
            return MegaId.Create(Convert.ToDouble(_idGenerator++));
        }
    }
}
