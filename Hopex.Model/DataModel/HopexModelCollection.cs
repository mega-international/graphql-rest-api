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
        private class ModelElementComparer : IEqualityComparer<IModelElement>
        {
            public bool Equals(IModelElement x, IModelElement y)
            {
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(IModelElement obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        protected IHopexDataModel _dataModel;
        protected readonly IModelElement _source;
        protected readonly IMegaRoot _iRoot;
        protected readonly GetCollectionArguments _getCollectionArguments;
        private readonly string _erql;
        private readonly List<Tuple<string, int>> _orderByClauses;

        protected MegaObject MegaObjectSource => _source?.MegaObject;
        protected IMegaObject IMegaObjectSource => _source?.IMegaObject;
        protected IClassDescription TargetClass => RelationshipDescription.TargetClass;

        internal static IModelCollection Create(IHopexDataModel domainModel, IRelationshipDescription relationshipDescription, IMegaRoot iRoot, IModelElement source, GetCollectionArguments getCollectionArguments)
        {
            switch (relationshipDescription.RoleId)
            {
                case MAEID_METACLASS_METAATRBIBUTE:
                    return new HopexMetaAttributeCollection(domainModel, relationshipDescription, iRoot, source, getCollectionArguments);
                case MAEID_METACLASS_SUBMETACLASS:
                    return new HopexRelatedClasssesCollection(domainModel, relationshipDescription, iRoot, source, getCollectionArguments, "~0fs9P5Ogg1fC[LowerClasses]");
                case MAEID_METACLASS_SUPERMETACLASS:
                    return new HopexRelatedClasssesCollection(domainModel, relationshipDescription, iRoot, source, getCollectionArguments, "~(es9P5ufg1fC[UpperClasses]");
                case MAEID_DESCRIBEDELEMENT_ABSTRACTDIAGRAM:
                    return new HopexDiagramCollection(domainModel, relationshipDescription, iRoot, source, getCollectionArguments);
            }
            return new HopexModelCollection(domainModel, relationshipDescription, iRoot, source, getCollectionArguments);
        }

        protected HopexModelCollection(IHopexDataModel domainModel, IRelationshipDescription schema, IMegaRoot iRoot, IModelElement source, GetCollectionArguments getCollectionArguments)
        {
            _dataModel = domainModel;
            RelationshipDescription = schema;
            _iRoot = iRoot;
            _source = source;
            _getCollectionArguments = getCollectionArguments;
            _erql = getCollectionArguments.Erql;
            _orderByClauses = getCollectionArguments.OrderByClauses;
        }

        public IRelationshipDescription RelationshipDescription { get; }

        public virtual IEnumerator<IModelElement> GetEnumerator()
        {
            if (_erql != null)
            {
                IMegaCollection items;
                switch (_orderByClauses?.Count ?? 0)
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
                if (_source != null)
                {
                    foreach (var item in ResolveByPath(_source, items))
                    {
                        yield return item;
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        yield return _dataModel.BuildElement(item, TargetClass);
                    }
                }
            }
            else if (_source == null)
            {
                var items = SortCollection(_iRoot, RelationshipDescription.TargetClass.Id);
                foreach (var item in items)
                {
                    yield return _dataModel.BuildElement(item, TargetClass);
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

        private IEnumerable<IModelElement> ResolveByPath(IModelElement currentModelElement, IMegaCollection foundItems = null)
        {
            var pathes = RelationshipDescription.Path;
            IEnumerable<IModelElement> sources = new List<IModelElement> { currentModelElement };
            for(var idx = 0; idx < pathes.Count(); ++idx)
            {
                var path = pathes[idx];
                var isLast = idx == pathes.Count() - 1;
                var isFirst = idx == 0;

                //Si on arrive au dernier path, on veut éviter de retourner des doublons partageant le même Id
                IEnumerable<IModelElement> targets = isLast ?
                    new HashSet<IModelElement>(new ModelElementComparer()) : new List<IModelElement> { };

                foreach(var source in sources)
                {
                    //On ne trie qu'à la dernière itération
                    IMegaCollection megaCollection = isLast ? SortCollection(source.IMegaObject, path.RoleId) : source.IMegaObject.GetCollection(path.RoleId);

                    IEnumerable<IMegaObject> megaObjects = megaCollection.GetType(path.TargetSchemaId).Where(obj =>
                    {
                        //On vérifie les conditions
                        if(!MetaAssociationConditionFilter(obj, path))
                        {
                            return false;
                        }

                        //Si on est à la fin, on fait le filtrage si le filtre existe, sinon tout est ok
                        if(isLast)
                        {
                            return foundItems?.Any(found => found.Id.Equals(obj.Id)) ?? true;
                        }
                        return true;
                    });

                    targets = targets.Concat(
                        megaObjects.Select(obj =>
                        source.BuildChildElement(obj, RelationshipDescription, idx)))
                        .ToList();
                }
                sources = targets;
            }
            return sources;
        }

        private IMegaCollection SortCollection(IMegaObject current, string roleId)
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
                        megaCollection = current.GetCollection(roleId,
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
