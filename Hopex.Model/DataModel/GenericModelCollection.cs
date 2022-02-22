using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.Model.DataModel
{
    class GenericModelCollection : IModelCollection
    {
        private readonly IModelElement _source;
        private readonly IHopexDataModel _domainModel;
        private readonly string _collectionMegaId;

        public GenericModelCollection(string collectionMegaId, IModelElement source, IHopexDataModel domainModel)
        {
            _collectionMegaId = Utils.NormalizeHopexId(collectionMegaId); ;
            _source = source;
            _domainModel = domainModel;
        }

        public IRelationshipDescription RelationshipDescription => throw new NotImplementedException();

        public IEnumerator<IModelElement> GetEnumerator()
        {
            var collection = _source.IMegaObject.GetCollection(_collectionMegaId);
            foreach (var item in collection)
            {
                yield return _domainModel.BuildElement(item, null, _source);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
