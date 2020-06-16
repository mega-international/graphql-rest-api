using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.DataModel
{
    internal class HopexRelatedClasssesCollection : HopexModelCollection
    {
        private readonly MegaId _collectionId;

        public HopexRelatedClasssesCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IMegaObject megaObject, GetCollectionArguments getCollectionArguments, MegaId collectionId)
            : base(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments)
        {
            _collectionId = collectionId;
        }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            var schemaElement = GetSchemaElement();
            var classDescription = _iRoot.GetClassDescription(_iSource.Id);
            return classDescription
                .GetCollection(_collectionId)
                .Select(cd => _iRoot.GetObjectFromId(cd.MegaUnnamedField))
                .Select(mo => new HopexModelElement(_dataModel, schemaElement, mo))
                .Where(_getCollectionArguments.AdHocPredicate)
                .GetEnumerator();
        }
    }
}
