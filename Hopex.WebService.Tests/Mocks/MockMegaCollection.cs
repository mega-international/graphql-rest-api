using Hopex.Model.Mocks;
using Mega.Macro.API;
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
    }
}
