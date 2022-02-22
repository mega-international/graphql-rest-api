using GraphQL;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Hopex.Model.DataModel
{
    internal class ClassCollectionDescription : IRelationshipDescription
    {
        public ClassCollectionDescription(string id, IClassDescription schema)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            Id = id;
            TargetClass = schema;
            Name = schema.Name;
            RoleId = schema.Id;
            Path = new [] { new PathDescription(Name, RoleId) };
        }
        public string Id { get; }
        public string Name { get; }
        public string RoleId { get; }
        public string Description { get; }
        public bool IsReadOnly { get; }
        public IPathDescription[] Path { get; }
        public IClassDescription ClassDescription { get; }
        public IClassDescription TargetClass { get; }

        string IRelationshipDescription.ReverseId => throw new NotImplementedException();

        public IEnumerable<ISetter> CreateSetters(object value)
        {
            if(value is Tuple<object, Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>>> pair)
            {
                if(pair.Item1 is IDictionary<string, object> dict)
                {
                    var resolver = pair.Item2;
                    var action = (CollectionAction)Enum.Parse(typeof(CollectionAction), dict ["action"].ToString(), true);
                    var list = (IEnumerable<object>)dict ["list"];
                    yield return CollectionSetter.Create(this, action, list, resolver);
                }
            }
        }
    }

    public class HopexDataModel : HasCollection, IHopexDataModel, IDisposable
    {
        private readonly MegaRoot _root;
        
        private readonly ILogger _logger;

        private IHopexMetaModel _metaModel;

        public Dictionary<string, IModelElement> TemporaryMegaObjects { get; }

        public HopexDataModel(IHopexMetaModel schema, MegaRoot root, IMegaRoot iRoot, ILogger logger) : base(iRoot)
        {
            _metaModel = schema;
            _root = root;
            _logger = logger;
            TemporaryMegaObjects = new Dictionary<string, IModelElement>();
        }

        public async Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id, IdTypeEnum idType)
        {
            return await GetElementByIdAsync(schema, null, id, idType, this);
        }

        public override Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            var schema = _metaModel.GetClassDescription(name);
            string id = null;
            var collection = HopexModelCollection.Create(this, new ClassCollectionDescription(id, schema), _iRoot, null, getCollectionArguments);
            return Task.FromResult(collection);
        }

        public Task<List<IModelElement>> SearchAllAsync(IResolveFieldContext<IHopexDataModel> ctx, IClassDescription genericClass)
        {
            if (ctx.Arguments.TryGetValue("filter", out var filterObject) && filterObject.Value is Dictionary<string, object> filter && filter.ContainsKey("text") && filter["text"] is string value && !string.IsNullOrWhiteSpace(value))
            {
                var minRange = 0;
                if (filter.ContainsKey("minRange") && filter["minRange"] is int minRangeValue)
                {
                    minRange = minRangeValue;
                }
                var maxRange = 1000;
                if (filter.ContainsKey("maxRange") && filter["maxRange"] is int maxRangeValue)
                {
                    if (maxRangeValue > maxRange)
                    {
                        try
                        {
                            ctx.Errors.Add(new ExecutionError($"Search method can only return a maximum of {maxRange} results."));
                        }
                        catch (Exception)
                        {
                            maxRangeValue = 1000;
                        }
                    }
                    maxRange = maxRangeValue;
                }
                var sortColumn = "Ranking";
                var sortDirection = "ASC";
                var orderByClause = "";
                if (ctx.Arguments.TryGetValue("orderBy", out var orderByObject))
                {
                    switch (orderByObject.Value)
                    {
                        case object[] orderBy:
                        {
                            if (orderBy[0] is Tuple<string, string> orderByValue)
                            {
                                sortColumn = orderByValue.Item1;
                                sortDirection = orderByValue.Item2;
                                orderByClause = $"{orderByValue.Item1} {orderByValue.Item2}";
                            }
                            break;
                        }
                        case List<object> orderByList:
                        {
                            sortColumn = "";
                            sortDirection = "orderByValue.Item2";
                            foreach (Tuple<string, string> orderByValue in orderByList)
                            {
                                sortColumn += orderByValue.Item1 + ",";
                                sortDirection += orderByValue.Item2 + ",";
                                orderByClause += $"{orderByValue.Item1} {orderByValue.Item2},";
                            }
                            sortColumn = sortColumn.Remove(sortColumn.Length - 1, 1);
                            sortDirection = sortDirection.Remove(sortDirection.Length - 1, 1);
                            orderByClause = orderByClause.Remove(orderByClause.Length - 1, 1);
                            break;
                        }
                    }
                }
                var languageId = "";
                if (ctx.Arguments.TryGetValue("language", out var languageValue) && languageValue.Value is IMegaObject language)
                {
                    languageId = language.GetPropertyValue(MetaAttributeLibrary._hexaidabs);
                }
                var searchAllRequest = new SearchAllRequestRoot
                {
                    Request = new SearchAllRequest
                    {
                        Value = value,
                        Language = languageId,
                        MinRange = minRange,
                        MaxRange = maxRange,
                        SortColumn = sortColumn,
                        SortDirection = sortDirection
                    }
                };
                var searchAllRequestString = JsonConvert.SerializeObject(searchAllRequest);
                var searchAllMacro = _iRoot.CurrentEnvironment.GetMacro("~w9D5uK4iI9n0[SearchRepository.GetResult]");
                searchAllMacro.Generate(_iRoot.NativeObject, null, searchAllRequestString, out object result);
                var searchAllResultRoot = JsonConvert.DeserializeObject<SearchAllResultRoot>(result.ToString());
                if (!string.IsNullOrEmpty(orderByClause))
                {
                    searchAllResultRoot.Results.OccResults.OccList = searchAllResultRoot.Results.OccResults.OccList.AsQueryable().OrderBy(orderByClause).ToList();
                }
                if (!string.IsNullOrWhiteSpace(searchAllResultRoot.Results.Message))
                {
                    throw new ExecutionError(searchAllResultRoot.Results.Message);
                }
                var collection =  new List<IModelElement>();
                foreach (var occ in searchAllResultRoot.Results.OccResults.OccList)
                {
                    var megaObject = _iRoot.GetObjectFromId(occ.ObjectId);
                    var element = BuildElement(megaObject, genericClass);
                    if(element.GetCrud().IsReadable)
                    {
                        collection.Add(element);
                    }
                }
                return Task.FromResult(collection);
            }
            throw new ExecutionError("In order to use the searchAll query, you must define a filter with a text attribute value");
        }

        public IModelElement BuildElement(IMegaObject megaObject, IClassDescription entity, IModelElement parent = null)
        {
            return new HopexModelElement(this, entity, megaObject, null, parent);
        }

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
        {
            if(schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if(string.IsNullOrEmpty(id) && (idType == IdTypeEnum.EXTERNAL || idType == IdTypeEnum.TEMPORARY))
            {
                throw new ExecutionError("Parameter id must be set");
            }

            var permissions = CrudComputer.GetCollectionMetaPermission(_iRoot, schema.Id);
            var settersList = setters.ToList();
            if(!permissions.IsCreatable || settersList.Any() && !permissions.IsUpdatable)
            {
                throw new ExecutionError("You are not allowed to perform this action");
            }

            var collection = _iRoot.GetCollection(schema.Id);
            var element = await CreateSingleElementAsync(schema, useInstanceCreator, id, idType, collection,
                mo => BuildElement(mo, schema), TemporaryMegaObjects, true);

            return await element.UpdateAsync(setters);
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if(string.IsNullOrEmpty(id) && (idType == IdTypeEnum.EXTERNAL || idType == IdTypeEnum.TEMPORARY))
            {
                throw new ExecutionError("Parameter id must be set");
            }
            var element = await GetElementByIdAsync(schema, id, idType);
            if(element == null)
            {
                throw new ExecutionError($"Element {schema.Name} not found with id {id}");
            }

            return await element.UpdateAsync(setters);
        }

        public async Task<IModelElement> CreateUpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if(string.IsNullOrEmpty(id) && (idType == IdTypeEnum.EXTERNAL || idType == IdTypeEnum.TEMPORARY))
            {
                throw new ExecutionError("Parameter id must be set");
            }

            if(!string.IsNullOrEmpty(id))
            {
                var element = await GetElementByIdAsync(schema, id, idType);
                if(element != null)
                {
                    return await element.UpdateAsync(setters);
                }
            }
            return await CreateElementAsync(schema, id, idType, useInstanceCreator, setters);
        }

        public Task<DeleteResultType> RemoveElementAsync(IEnumerable<IMegaObject> objectsToDelete, bool isCascade = false)
        {
            var removedElementCount = 0;
            foreach (var megaObject in objectsToDelete)
            {
                var elementPermissions = CrudComputer.GetCrud(megaObject);
                if (!elementPermissions.IsDeletable)
                {
                    throw new ExecutionError($"You are not allowed to perform this action on this object ({megaObject.MegaField})");
                }
                if (isCascade)
                {
                    megaObject.CallMethod("~9InwiiVr3ba0[QueryDelete]", "SilentMode", "");
                }
                else
                {
                    megaObject.Delete();
                    removedElementCount++;
                }
            }
            return Task.FromResult(new DeleteResultType {DeletedCount = removedElementCount});
        }

        public async Task<DeleteResultType> RemoveElementAsync(IEnumerable<IModelElement> elementsToDelete, bool isCascade = false)
        {
            var objectsToDelete = elementsToDelete.Select(e => e.IMegaObject).ToList();
            return await RemoveElementAsync(objectsToDelete, isCascade);
        }

        public void Dispose()
        {
            (_root as IDisposable)?.Dispose();
        }

        public void CheckObjectCreation(object obj)
        {
            if(obj is MegaObject megaObj)
            {
                if (!megaObj.Exists)
                {
                    throw new ExecutionError($"Object {megaObj.Id?.ToString() ?? "null"} is empty, creation failed");
                }
            }
            else if(obj is IMegaObject iMegaObj)
            {
                if (!iMegaObj.Exists)
                {
                    throw new ExecutionError($"Object {iMegaObj.Id?.ToString() ?? "null"} is empty, creation failed");
                }
            }
            else
            {
                throw new ExecutionError($"Object {obj.ToString()} is not a MegaObject");
            }
        }
    }
}
