using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.DataModel
{
    internal class HopexModelCollection : IModelCollection, IDisposable
    {
        private HopexDataModel _dataModel;
        private readonly MegaRoot _root;
        private readonly IMegaRoot _iRoot;
        private readonly MegaObject _source;
        private readonly string _erql;
        private readonly List<Tuple<string, int>> _orderByClauses;

        public HopexModelCollection(HopexDataModel domainModel, IRelationshipDescription schema, MegaRoot root, IMegaRoot iRoot, MegaObject source, string erql, List<Tuple<string, int>> orderByClauses)
        {
            _dataModel = domainModel;
            RelationshipDescription = schema;
            _root = root;
            _iRoot = iRoot;
            _source = source;
            _erql = erql;
            _orderByClauses = orderByClauses;
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public IEnumerator<IModelElement> GetEnumerator()
        {
            var target = RelationshipDescription.Path.Last();
            var schemaElement = RelationshipDescription.ClassDescription.MetaModel.GetClassDescription(target.TargetSchemaName);
            if (RelationshipDescription.Id == "EIedE)fyA1y0")
            {
                var diagrams = MegaWrapperObject.CastIfAny<MegaCollection>(_source.NativeObject.GetDescribingDiagrams());
                foreach (var diagram in diagrams)
                {
                    yield return new HopexModelElement(_dataModel, schemaElement, diagram);
                }
                yield break;
            }
            if (_erql != null)
            {
                MegaCollection items;
                switch (_orderByClauses.Count)
                {
                    case 0:
                        items = _root.GetSelection(_erql);
                        break;
                    case 1:
                        items = _root.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1);
                        break;
                    case 2:
                        items = _root.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1);
                        break;
                    default:
                        items = _root.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1);
                        break;
                }
                foreach (MegaObject item in items)
                {
                    yield return new HopexModelElement(_dataModel, schemaElement, item);
                }
            }
            else if (_source == null)
            {
                var items = SortCollection(_iRoot, _root, RelationshipDescription.ClassDescription.Id);
                foreach (var item in items)
                {
                    yield return new HopexModelElement(_dataModel, schemaElement, item);
                }
            }
            else
            {
                foreach (var item in ResolveByPath(_source))
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<IModelElement> ResolveByPath(MegaObject current, int level = 0, HashSet<MegaId> distinctItemsIds = null)
        {
            MegaCollection megaCollection;
            var hop = RelationshipDescription.Path[level];

            if (level == RelationshipDescription.Path.Count() - 1) // Last
            {
                megaCollection = SortCollection(current, hop.RoleId);
            }
            else
            {
                megaCollection = current.GetCollection(hop.RoleId); // Pas de sort sur les segments intermédiaires
            }

            var items = MegaWrapperObject.Cast<MegaCollection>(megaCollection.NativeObject.GetType(hop.TargetSchemaId.ToString()));

            if (distinctItemsIds == null)
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
                        if (MetaAssociationConditionFilter(item, hop))
                        {
                            if (distinctItemsIds.Add(item.Id))
                            {
                                yield return new HopexModelElement(_dataModel, schemaElement, item);
                            }
                        }
                    }
                }
                else
                {
                    foreach (MegaObject item in items)
                    {
                        if (MetaAssociationConditionFilter(item, hop))
                        {
                            foreach (var item2 in ResolveByPath(item, level + 1, distinctItemsIds))
                            {
                                yield return item2;
                            }
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

        private MegaCollection SortCollection(MegaObject current, string roleId)
        {
            MegaCollection megaCollection;
            if (_orderByClauses != null)
            {
                switch (_orderByClauses.Count)
                {
                    case 0:
                        megaCollection = current.GetCollection(roleId);
                        break;
                    case 1:
                        megaCollection = current.GetCollection(roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1);
                        break;
                    case 2:
                        megaCollection = current.GetCollection(roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1);
                        break;
                    default:
                        megaCollection = current.NativeObject.GetCollection(
                            roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1);
                        break;
                }
            }
            else
            {
                megaCollection = current.GetCollection(roleId);
            }

            return megaCollection;
        }

        private IMegaCollection SortCollection(IMegaObject current, MegaObject nativeObject, string roleId)
        {
            IMegaCollection megaCollection;
            if (_orderByClauses != null)
            {
                switch (_orderByClauses.Count)
                {
                    case 0:
                        megaCollection = current.GetCollection(roleId);
                        break;
                    case 1:
                        megaCollection = current.GetCollection(roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1);
                        break;
                    case 2:
                        megaCollection = current.GetCollection(roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1);
                        break;
                    default:
                        megaCollection = nativeObject.NativeObject.GetCollection(
                            roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1);
                        break;
                }
            }
            else
            {
                megaCollection = current.GetCollection(roleId);
            }

            return megaCollection;
        }

        private static bool MetaAssociationConditionFilter(MegaObject item, IPathDescription hop)
        {
            if (hop.Condition == null || string.IsNullOrEmpty(hop.Condition.RoleId) || string.IsNullOrEmpty(hop.Condition.ObjectFilterId))
            {
                return true;
            }
            return item.GetPropertyValue(Utils.NormalizeHopexId(hop.Condition.RoleId)) == hop.Condition.ObjectFilterId;
        }

        public void Dispose()
        {
            _dataModel = null;
        }
    }
}
