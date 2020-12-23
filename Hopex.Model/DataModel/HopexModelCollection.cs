using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static Hopex.Model.MetaModel.Constants;

namespace Hopex.Model.DataModel
{
    internal class HopexModelCollection : IModelCollection, IDisposable
    {
        protected IHopexDataModel _dataModel;
        private readonly MegaRoot _root;
        protected readonly MegaObject _source;
        protected readonly IMegaRoot _iRoot;
        protected readonly IMegaObject _iSource;
        protected readonly GetCollectionArguments _getCollectionArguments;
        private readonly string _erql;
        private readonly List<Tuple<string, int>> _orderByClauses;

        internal static IModelCollection Create(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IMegaObject megaObject, GetCollectionArguments getCollectionArguments)
        {
            switch (relationshipDescription.RoleId)
            {
                case MAEID_METACLASS_METAATRBIBUTE:
                    return new HopexMetaAttributeCollection(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments);
                case MAEID_METACLASS_SUBMETACLASS:
                    return new HopexRelatedClasssesCollection(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments, "~0fs9P5Ogg1fC[LowerClasses]");
                case MAEID_METACLASS_SUPERMETACLASS:
                    return new HopexRelatedClasssesCollection(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments, "~(es9P5ufg1fC[UpperClasses]");
                case MAEID_DESCRIBEDELEMENT_ABSTRACTDIAGRAM:
                    return new HopexDiagramCollection(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments);
            }
            return new HopexModelCollection(domainModel, relationshipDescription, iRoot, megaObject, getCollectionArguments);
        }

        protected HopexModelCollection(IHopexDataModel domainModel, IRelationshipDescription schema, IMegaRoot iRoot, IMegaObject iSource, GetCollectionArguments getCollectionArguments)
        {
            _dataModel = domainModel;
            RelationshipDescription = schema;
            _iRoot = iRoot;
            _iSource = iSource;
            _getCollectionArguments = getCollectionArguments;
            _erql = getCollectionArguments.Erql;
            _orderByClauses = getCollectionArguments.OrderByClauses;

            if (iSource is RealMegaObject)
            {
                _source = ((RealMegaObject)iSource).RealObject;
            }
            if (iRoot is RealMegaRoot)
            {
                _root = ((RealMegaRoot)iRoot).RealRoot;
            }
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public virtual IEnumerator<IModelElement> GetEnumerator()
        {
            var schemaElement = GetSchemaElement();
            if (_erql != null)
            {
                IMegaCollection items;
                switch (_orderByClauses.Count)
                {
                    case 0:
                        items = _iRoot.GetSelection(_erql);
                        break;
                    case 1:
                        items = _iRoot.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1);
                        break;
                    case 2:
                        items = _iRoot.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1);
                        break;
                    default:
                        items = _iRoot.GetSelection(_erql,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1);
                        break;
                }
                if (_iSource != null)
                {
                    foreach (var item in ResolveByPath(_iSource, items))
                    {
                        yield return item;
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        yield return new HopexModelElement(_dataModel, schemaElement, item);
                    }
                }
            }
            else if (_iSource == null)
            {
                var items = SortCollection(_iRoot, _root, RelationshipDescription.ClassDescription.Id);
                foreach (var item in items)
                {
                    yield return new HopexModelElement(_dataModel, schemaElement, item);
                }
            }
            else
            {
                foreach (var item in ResolveByPath(_iSource))
                {
                    yield return item;
                }
            }
        }

        protected IClassDescription GetSchemaElement()
        {
            var target = RelationshipDescription.Path.Last();
            var schemaElement = RelationshipDescription.ClassDescription.MetaModel.GetClassDescription(target.TargetSchemaName);
            return schemaElement;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<IModelElement> ResolveByPath(IMegaObject iCurrent, IMegaCollection foundItems = null, int level = 0, HashSet<MegaId> distinctItemsIds = null)
        {
            MegaObject current = null;
            if (iCurrent is RealMegaObject)
            {
                current = ((RealMegaObject)iCurrent).RealObject;
            }
            IMegaCollection megaCollection;
            var hop = RelationshipDescription.Path[level];

            List<IMegaObject> items;
            if (level == RelationshipDescription.Path.Count() - 1) // Last
            {
                megaCollection = SortCollection(iCurrent, current, hop.RoleId);
                items = megaCollection.GetType(hop.TargetSchemaId).ToList();
                var foundItemsInItems = new List<MegaId>();
                if (foundItems != null)
                {
                    foreach (var item in items)
                    {
                        foreach (var foundItem in foundItems)
                        {
                            if (Equals(item.Id, foundItem.Id))
                            {
                                foundItemsInItems.Add(item.Id);
                            }
                        }
                    }
                    items.RemoveAll(x => !foundItemsInItems.Contains(x.Id));
                }
            }
            else
            {
                megaCollection = iCurrent.GetCollection(hop.RoleId); // Pas de sort sur les segments intermédiaires
                items = megaCollection.GetType(hop.TargetSchemaId).ToList();
            }    

            if (distinctItemsIds == null)
            {
                distinctItemsIds = new HashSet<MegaId>();
            }
            try
            {
                var schemaElement = RelationshipDescription.ClassDescription.MetaModel.GetClassDescription(hop.TargetSchemaName);
                if (level == RelationshipDescription.Path.Count() - 1) // Last
                {
                    foreach (IMegaObject item in items)
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
                    foreach (IMegaObject item in items)
                    {
                        if (MetaAssociationConditionFilter(item, hop))
                        {
                            foreach (var item2 in ResolveByPath(item, foundItems, level + 1, distinctItemsIds))
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
                    iCurrent?.Dispose();
                }
            }
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
                        megaCollection = new RealMegaCollection(nativeObject.NativeObject.GetCollection(
                            roleId,
                            _orderByClauses[0].Item2, _orderByClauses[0].Item1,
                            _orderByClauses[1].Item2, _orderByClauses[1].Item1,
                            _orderByClauses[2].Item2, _orderByClauses[2].Item1));
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

        private static bool MetaAssociationConditionFilter(IMegaObject item, IPathDescription hop)
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
