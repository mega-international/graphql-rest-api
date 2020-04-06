using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.Model.DataModel
{
    class GenericModelCollection : IModelCollection
    {
        private readonly IMegaObject _iMegaObject;
        private readonly HopexDataModel _domainModel;
        private string _collectionMegaId;

        public GenericModelCollection(string collectionMegaId, IMegaObject iMegaObject, HopexDataModel domainModel)
        {
            _collectionMegaId = Utils.NormalizeHopexId(collectionMegaId); ;
            _iMegaObject = iMegaObject;
            _domainModel = domainModel;
        }

        public IRelationshipDescription RelationshipDescription => throw new NotImplementedException();

        public IEnumerator<IModelElement> GetEnumerator()
        {
            var collection = _iMegaObject.GetCollection(_collectionMegaId);
            foreach (var item in collection)
            {
                yield return new HopexModelElement(_domainModel, null, item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
