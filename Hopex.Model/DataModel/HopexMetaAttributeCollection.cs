using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.DataModel
{
    internal class HopexMetaAttributeCollection : HopexModelCollection
    {
        private const string MCID_METAATTRIBUTE = "O20000000Y10";

        public HopexMetaAttributeCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IMegaObject megaObject, GetCollectionArguments getCollectionArguments)
            : base(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments)
        { }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            var schemaElement = GetSchemaElement();
            var toolkit = _iRoot.CurrentEnvironment.Toolkit;
            var classDescription = _iRoot.GetClassDescription(_iSource.Id);
            var collectionDescription = classDescription.GetCollection("~1fs9P5egg1fC[Description]").Item(1);
            return collectionDescription
                .GetCollection("~7fs9P58ig1fC[Properties]")
                .Select(pd => _iRoot.GetObjectFromId(pd.MegaUnnamedField))
                .Where(po => toolkit.IsSameId(po.GetClassId(), MCID_METAATTRIBUTE))
                .Select(po => new HopexModelElement(_dataModel, schemaElement, po, null, _iSource))
                .GetEnumerator();
        }
    }
}
