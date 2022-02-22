using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.DataModel
{
    internal class HopexMetaAttributeCollection : HopexModelCollection
    {
        private const string MCID_METAATTRIBUTE = "O20000000Y10";

        public HopexMetaAttributeCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IModelElement source, GetCollectionArguments getCollectionArguments)
            : base(domainModel, relationshipDescription, iRoot, source, getCollectionArguments)
        { }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            if(IMegaObjectSource == null)
            {
                throw new NullReferenceException("MegaObjectSource is null");
            }

            var toolkit = _iRoot.CurrentEnvironment.Toolkit;
            var classDescription = _iRoot.GetClassDescription(IMegaObjectSource.Id);
            var collectionDescription = classDescription.GetCollection("~1fs9P5egg1fC[Description]").Item(1);
            return collectionDescription
                .GetCollection("~7fs9P58ig1fC[Properties]")
                .Select(pd => _iRoot.GetObjectFromId(pd.MegaUnnamedField))
                .Where(po => toolkit.IsSameId(po.GetClassId(), MCID_METAATTRIBUTE))
                .Select(po => new HopexModelElement(_dataModel, TargetClass, po, null, _source))
                .GetEnumerator();
        }
    }
}
