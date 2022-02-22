using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.Mocks
{
    internal class MockModelCollection : IModelCollection
    {
        private int _count;
        private readonly List<IModelElement> _elements = new List<IModelElement>();

        public MockModelCollection(MockDataModel dataModel, IRelationshipDescription relationshipDescription) :
            this(dataModel, dataModel.MetaModel.GetClassDescription(relationshipDescription.Path.Last().TargetSchemaName))
        {
            RelationshipDescription = relationshipDescription;
        }

        public MockModelCollection(MockDataModel dataModel, IClassDescription targetSchema)
        {
            _count = MockDataModel.MaxCollectionSize;
            for (var ix = 0; ix < _count; ix++)
            {
                var elem = dataModel.CreateElement(targetSchema, 100+ix);
                _elements.Add(elem);
            }
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public IEnumerator<IModelElement> GetEnumerator()
        {
            foreach (var elem in _elements)
            {
                yield return elem;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal bool Remove(MegaId id, bool cascade)
        {
            var e = _elements.FirstOrDefault(el => el.Id == id);
            if (e != null)
            {
                _elements.Remove(e);
                _count--;
                return true;
            }
            return false;
        }

        internal void Add(IModelElement e)
        {
            _elements.Add(e);
            _count++;
        }

        internal void Clear()
        {
            _elements.Clear();
            _count = 0;
        }
    }
}
