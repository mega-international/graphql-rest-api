using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.DataModel
{
    internal class HopexRelatedClasssesCollection : HopexModelCollection
    {
        private readonly MegaId _collectionId;

        public HopexRelatedClasssesCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IModelElement source, GetCollectionArguments getCollectionArguments, MegaId collectionId)
            : base(domainModel, relationshipDescription, iRoot, source, getCollectionArguments)
        {
            _collectionId = collectionId;
        }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            if(IMegaObjectSource == null)
            {
                throw new NullReferenceException("MegaObjectSource is null");
            }

            var classDescription = _iRoot.GetClassDescription(IMegaObjectSource.Id);
            return classDescription
                .GetCollection(_collectionId)
                .Select(cd => _iRoot.GetObjectFromId(cd.MegaUnnamedField))
                .Select(mo => _dataModel.BuildElement(mo, TargetClass))
                .Where(_getCollectionArguments.AdHocPredicate)
                .GetEnumerator();
        }
    }
}
