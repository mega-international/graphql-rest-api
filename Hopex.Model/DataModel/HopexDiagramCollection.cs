using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Model.DataModel
{
    internal class HopexDiagramCollection : HopexModelCollection
    {
        public HopexDiagramCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IMegaObject megaObject, GetCollectionArguments getCollectionArguments)
            : base(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments)
        { }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            var schemaElement = GetSchemaElement();
            var diagrams = MegaWrapperObject.CastIfAny<MegaCollection>(_source.NativeObject.GetDescribingDiagrams());
            foreach (var diagram in diagrams)
            {
                yield return new HopexModelElement(_dataModel, schemaElement, diagram);
            }
            yield break;
        }
    }
}
