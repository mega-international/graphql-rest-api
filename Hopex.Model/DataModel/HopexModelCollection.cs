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
        private readonly MegaRoot _root;
        private readonly string _erql;
        private readonly List<Tuple<string, int>> _orderByClauses;

        public HopexModelCollection(HopexDataModel domainModel, IRelationshipDescription schema, MegaRoot source, string erql, List<Tuple<string, int>> orderByClauses)
        {
            _dataModel = domainModel;
            RelationshipDescription = schema;
            _root = source;
            _erql = erql;
            _orderByClauses = orderByClauses;
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public IEnumerator<IModelElement> GetEnumerator()
        {
            var target = RelationshipDescription.Path.Last();
            var schemaElement = RelationshipDescription.ClassDescription.MetaModel.GetClassDescription(target.TargetSchemaName);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<IModelElement> ResolveByPath(MegaObject current, int level = 0, HashSet<MegaId> distinctItemsIds = null)
        {
            MegaCollection megaCollection;
            var hop = RelationshipDescription.Path[level];

            if (_orderByClauses != null)
            {
                switch (_orderByClauses.Count)
                {
                    case 0:
                        megaCollection = current.GetCollection(hop.RoleId);
                        break;
                    case 1:
                        megaCollection = current.GetCollection(hop.RoleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1);
                        break;
                    case 2:
                        megaCollection = current.GetCollection(hop.RoleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1);
                        break;
                    default:
                        megaCollection = current.NativeObject.GetCollection(
                            hop.RoleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1);
                        break;
                }
            }
            else
            {
                megaCollection = current.GetCollection(hop.RoleId);
            }

            var items = MegaWrapperObject.Cast<MegaCollection>(megaCollection.NativeObject.GetType(hop.TargetSchemaId.ToString()));
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
                        if(MetaAssociationConditionFilter(item, hop))
                        {
                            if(distinctItemsIds.Add(item.Id))
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
                        if(MetaAssociationConditionFilter(item, hop))
                        {
                            foreach(var item2 in ResolveByPath(item, level + 1, distinctItemsIds))
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

        private static bool MetaAssociationConditionFilter(MegaObject item, IPathDescription hop)
        {
            if(hop.Condition == null || string.IsNullOrEmpty(hop.Condition.RoleId) || string.IsNullOrEmpty(hop.Condition.ObjectFilterId))
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

