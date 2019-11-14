using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.DataModel
{
    internal class HopexModelCollection : IModelCollection, IDisposable
    {
        private HopexDataModel _dataModel;
        private readonly MegaObject _source;

        public HopexModelCollection(HopexDataModel domainModel, IRelationshipDescription schema, MegaObject source)
        {
            _dataModel = domainModel;
            RelationshipDescription = schema;
            _source = source;
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public IEnumerator<IModelElement> GetEnumerator()
        {
            foreach (var item in ResolveByPath(_source))
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<IModelElement> ResolveByPath(MegaObject current, int level = 0, HashSet<MegaId> distinctItemsIds = null)
        {
            var hop = RelationshipDescription.Path[level];
            //TODO: replace by: var items = current.GetCollection(hop.RoleId).Filter(hop.TargetSchemaId.ToString())
            var items = MegaWrapperObject.Cast<MegaCollection>(current.GetCollection(hop.RoleId).NativeObject.GetType(hop.TargetSchemaId.ToString()));
            if(distinctItemsIds == null)
            {
                distinctItemsIds = new HashSet<MegaId>();
            }
            try
            {
                var schemaElement = RelationshipDescription.ClassDescription.MetaModel.GetClassDescription(hop.TargetSchemaName);
                if (level == RelationshipDescription.Path.Count() - 1) // Last
                {
                    foreach (MegaObject item in items)
                    {
                        if(distinctItemsIds.Add(item.Id))
                        {
                            yield return new HopexModelElement(_dataModel, schemaElement, item);
                        }
                    }
                }
                else
                {
                    foreach (MegaObject item in items)
                    {
                        foreach (var item2 in ResolveByPath(item, level + 1, distinctItemsIds))
                        {
                            yield return item2;
                        }
                    }
                }
            }
            finally
            {
                if (level != 0)
                {
                    (current as IDisposable)?.Dispose();
                }
                (items as IDisposable)?.Dispose();
            }
        }

        public void Dispose()
        {
            _dataModel = null;
            (_source as IDisposable)?.Dispose();
        }
    }
}

