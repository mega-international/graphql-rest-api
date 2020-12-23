using GraphQL;
using GraphQL.Types;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Enums;
using Mega.Macro.API.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            ClassDescription = schema;
            Name = schema.Name;
            RoleId = schema.Id;
            Path = new [] { new PathDescription(Name, RoleId) };
        }
        public string Id { get; }
        public string Name { get; }
        public string RoleId { get; }
        public string Description { get; }
        public IPathDescription[] Path { get; }
        public IClassDescription ClassDescription { get; }
        public IClassDescription TargetClass { get; }

        string IRelationshipDescription.ReverseId => throw new NotImplementedException();
    }

    public class HopexDataModel : IHopexDataModel, IDisposable
    {
        private readonly MegaRoot _root;
        private readonly IMegaRoot _iRoot;
        private ILogger _logger;

        public IHopexMetaModel MetaModel { get; }

        public Dictionary<string, IModelElement> TemporaryMegaObjects { get; }

        public HopexDataModel(IHopexMetaModel schema, MegaRoot root, IMegaRoot iRoot, ILogger logger)
        {
            MetaModel = schema;
            _root = root;
            _iRoot = iRoot;
            _logger = logger;
            TemporaryMegaObjects = new Dictionary<string, IModelElement>();
        }

        public Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id, IdTypeEnum idType)
        {
            IMegaObject obj = null;
            switch (idType)
            {
                case IdTypeEnum.INTERNAL:
                    if (schema?.Id != null)
                        obj = _iRoot.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{id}\"").FirstOrDefault();
                    else
                        obj = _iRoot.GetObjectFromId(id);
                    break;
                case IdTypeEnum.EXTERNAL:
                    obj = _iRoot.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"").FirstOrDefault();
                    break;
            }
            if (obj == null || !obj.Exists)
            {
                return Task.FromResult<IModelElement>(null);
            }
            var modelElement = new HopexModelElement(this, schema, obj, obj.MegaUnnamedField);
            return Task.FromResult<IModelElement>(modelElement);
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments)
        {
            var schema = MetaModel.GetClassDescription(name);
            string id = null;
            if (relationshipName != null)
            {
                var rel = schema.Relationships.First(r => r.Name == relationshipName);
                id = rel.Id;
            }
            var collection = HopexModelCollection.Create(this, new ClassCollectionDescription(id, schema), _iRoot, null, getCollectionArguments);
            return Task.FromResult(collection);
        }

        public Task<List<IModelElement>> SearchAllAsync(ResolveFieldContext<IHopexDataModel> ctx)
        {
            if (ctx.Arguments.TryGetValue("filter", out var filterObject) && filterObject is Dictionary<string, object> filter && filter.ContainsKey("text") && filter["text"] is string value && !string.IsNullOrWhiteSpace(value))
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
                    switch (orderByObject)
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
                if (ctx.Arguments.TryGetValue("language", out var languageValue) && languageValue is IMegaObject language)
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
                    var element = new HopexModelElement(this, null, megaObject);
                    if(element.GetCrud().IsReadable)
                    {
                        collection.Add(element);
                    }
                }
                return Task.FromResult(collection);
            }
            throw new ExecutionError("In order to use the searchAll query, you must define a filter with a text attribute value");
        }

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var permissions = CrudComputer.GetCollectionMetaPermission(_iRoot, schema.Id);
            if (!permissions.IsCreatable || setters.Any() && !permissions.IsUpdatable)
            {
                throw new ExecutionError("You are not allowed to perform this action");
            }

            var collection = _iRoot.GetCollection(schema.Id);
            return await CreateElementFromParentAsync(schema, id, idType, useInstanceCreator, setters, collection);
        }

        public async Task<IModelElement> CreateElementFromParentAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters, IMegaCollection iColParent)
        {
            HopexModelElement element;
            if (Equals(schema.Id, "~UkPT)TNyFDK5") || Equals(schema.Id, "~aMRn)bUIGjX3"))
            {
                var filePath = Path.GetTempPath() + "empty.txt";
                File.WriteAllText(filePath, "");
                var instanceCreator = iColParent.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                instanceCreator.InvokePropertyPut("~vjh4n6oyFTKK[Localisation du fichier]", filePath);
                var newId = MegaId.Create(instanceCreator.Create());
                var item = iColParent.Item(newId);
                CheckObjectCreation(item);
                element = new HopexModelElement(this, schema, item);
            }
            else
            {
                if (!string.IsNullOrEmpty(id) && idType == IdTypeEnum.EXTERNAL)
                {
                    var obj = _iRoot.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"").FirstOrDefault();
                    if (obj != null && obj.Exists)
                    {
                        throw new ExecutionError($"Cannot create {schema.Name} with external identifier: {id}, this value is already used on element: {obj.MegaField}");
                    }
                }
                if (useInstanceCreator)
                {
                    var instanceCreator = iColParent.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                    instanceCreator.Mode = WizardCreateMode.Batch;
                    var newId = MegaId.Create(instanceCreator.Create());
                    IMegaObject item = iColParent.Item(newId);
                    CheckObjectCreation(item);
                    if (!string.IsNullOrEmpty(id))
                    {
                        switch (idType)
                        {
                            case IdTypeEnum.INTERNAL:
                                throw new ExecutionError("You cannot set the id in BUSINESS mode.");
                            case IdTypeEnum.EXTERNAL:
                                item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                break;
                        }
                    }
                    element = new HopexModelElement(this, schema, item);
                }
                else
                {
                    IMegaObject item = null;
                    if (!string.IsNullOrEmpty(id))
                    {
                        switch (idType)
                        {
                            case IdTypeEnum.INTERNAL:
                                var megaId = MegaId.Create($"~{id}");
                                item = iColParent.Create(megaId);
                                CheckObjectCreation(item);
                                break;
                            case IdTypeEnum.EXTERNAL:
                                item = iColParent.Create();
                                CheckObjectCreation(item);
                                var externalIdentifierPermissions = new CrudResult(item.CallFunctionString("~R2mHVReGFP46[WFQuery]", "~CFmhlMxNT1iE[ExternalIdentifier]"));
                                if (!externalIdentifierPermissions.IsUpdatable)
                                {
                                    item.Delete();
                                    throw new ExecutionError($"You are not allowed to create {schema.Name} with id type EXTERNAL");
                                }
                                item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                break;
                            case IdTypeEnum.TEMPORARY:
                                if (TemporaryMegaObjects.ContainsKey(id))
                                {
                                    item = TemporaryMegaObjects[id].IMegaObject;
                                }
                                else
                                {
                                    item = iColParent.Create();
                                    CheckObjectCreation(item);
                                    TemporaryMegaObjects.Add(id, new HopexModelElement(this, schema, item));    
                                }
                                break;
                        }
                    }
                    else
                    {
                        item = iColParent.Create();
                        CheckObjectCreation(item);
                    }
                    element = new HopexModelElement(this, schema, item);
                }
            }
            await element.UpdateElement(setters);
            return element;
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var getElementByIdAsyncTask = GetElementByIdAsync(schema, id, idType);
            if (getElementByIdAsyncTask == null)
            {
                throw new ExecutionError($"Element {schema.Name} not found with id {id}");
            }
            var element = await getElementByIdAsyncTask;
            if (!(element is HopexModelElement))
            {
                throw new ExecutionError($"Element {schema.Name} not found with id {id}");
            }

            var elementPermissions = element.GetCrud();
            if (! elementPermissions.IsUpdatable)
            {
                throw new ExecutionError($"You are not allowed to perform this action on this object ({element.MegaObject.MegaField})");
            }

            return await ProcessMutation("update", element.IMegaObject.MegaField, async() =>
            {
                await (element as HopexModelElement).UpdateElement(setters);
                return element;
            });
        }

        public async Task<IModelElement> CreateUpdateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }
            var permissions = CrudComputer.GetCollectionMetaPermission(_iRoot, schema.Id);
            var settersList = setters.ToList();
            if (!permissions.IsCreatable || settersList.Any() && !permissions.IsUpdatable)
            {
                throw new ExecutionError("You are not allowed to perform this action");
            }

            if(string.IsNullOrEmpty(id) && (idType == IdTypeEnum.EXTERNAL || idType == IdTypeEnum.TEMPORARY))
            {
                throw new ExecutionError("Parameter id must be set");
            }

            if(!string.IsNullOrEmpty(id))
            {
                IMegaObject obj = null;
                switch (idType)
                {
                    case IdTypeEnum.INTERNAL:
                        obj = _iRoot.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{id}\"").FirstOrDefault();
                        break;
                    case IdTypeEnum.EXTERNAL:
                        obj = _iRoot.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"").FirstOrDefault();
                        break;
                    case IdTypeEnum.TEMPORARY:
                        if (TemporaryMegaObjects.ContainsKey(id))
                        {
                            obj = TemporaryMegaObjects[id].IMegaObject;
                        }
                        break;
                }
                if(obj != null && obj.Exists)
                {
                    return await UpdateElementAsync(schema, obj.MegaUnnamedField, IdTypeEnum.INTERNAL, setters);
                }
            }
            return await CreateElementAsync(schema, id, idType, useInstanceCreator, setters);
        }

        public Task<DeleteResultType> RemoveElementAsync(List<IMegaObject> objectsToDelete, bool isCascade = false)
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

        private async Task<IModelElement> ProcessMutation(string mutationName, string megaField, Func<Task<IModelElement>> mutation)
        {
            return await mutation();
        }

        private void PublishSession()
        {
            var result = _iRoot.CallFunctionString("~lcE6jbH9G5cK", "{\"instruction\":\"PUBLISHINSESSION\"}");
            if (result == null || !result.Contains("SESSION_PUBLISH"))
            {
                throw new Exception("Session wasn't published");
            }
            _logger.LogInformation("Session published");
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
