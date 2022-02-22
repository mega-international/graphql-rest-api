using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;

namespace Hopex.Model.DataModel
{
    internal class HopexDiagramCollection : HopexModelCollection
    {
        public HopexDiagramCollection(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IModelElement source, GetCollectionArguments getCollectionArguments)
            : base(domainModel, relationshipDescription, iRoot, source, getCollectionArguments)
        { }

        public override IEnumerator<IModelElement> GetEnumerator()
        {
            if(MegaObjectSource == null)
            {
                throw new NullReferenceException("MegaObjectSource is null");
            }

            var diagrams = MegaWrapperObject.CastIfAny<MegaCollection>(MegaObjectSource.CallFunction<MegaCollection>("GetDescribingDiagrams"));
            foreach (MegaObject diagram in diagrams)
            {
                yield return _dataModel.BuildElement(new RealMegaObject(diagram), TargetClass);
            }
        }
    }
}
