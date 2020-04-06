using GraphQL;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Enums;
using Mega.Macro.API.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mega.Macro.API.Library;

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
    }

    public class HopexDataModel : IHopexDataModel, IDisposable
    {
        private readonly MegaRoot _root;
        private readonly IMegaRoot _iRoot;
        private ILogger _logger;

        public IHopexMetaModel MetaModel { get; }

        public HopexDataModel(IHopexMetaModel schema, MegaRoot root, IMegaRoot iRoot, ILogger logger)
        {
            MetaModel = schema;
            _root = root;
            _iRoot = iRoot;
            _logger = logger;
        }

        public Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id, IdTypeEnum idType)
        {
            IMegaObject obj = null;
            switch (idType)
            {
                case IdTypeEnum.INTERNAL:
                    if (schema.Id != null)
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
                return null;
            }
            var modelElement = new HopexModelElement(this, schema, obj, obj.MegaUnnamedField);
            return Task.FromResult<IModelElement>(modelElement);
        }

        public Task<IModelCollection> GetCollectionAsync(string name, string erql, List<Tuple<string, int>> orderByClauses, string relationshipName)
        {
            var schema = MetaModel.GetClassDescription(name);
            string id = null;
            if(relationshipName != null)
            {
                var rel = schema.Relationships.First(r => r.Name == relationshipName);
                id = rel.Id;
            }
            return Task.FromResult<IModelCollection>(new HopexModelCollection(this, new ClassCollectionDescription(id, schema), _root, _iRoot, null, erql, orderByClauses));
        }

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters)
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

            return await ProcessMutation("create", null, async() =>
            {
                HopexModelElement element;
                if(Equals(schema.Id, "~UkPT)TNyFDK5") || Equals(schema.Id, "~aMRn)bUIGjX3"))
                {
                    var filePath = Path.GetTempPath() + "empty.txt";
                    File.WriteAllText(filePath, "");
                    var instanceCreator = _root.GetCollection(schema.Id).CallFunction<MegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                    instanceCreator.InvokePropertyPut("~vjh4n6oyFTKK[Localisation du fichier]", filePath);
                    var newId = MegaId.Create(instanceCreator.InvokeFunction<double>("Create"));
                    MegaObject item = _root.GetCollection(schema.Id).Item(newId);
                    element = new HopexModelElement(this, schema, item);
                }
                else
                {
                    if(useInstanceCreator)
                    {
                        var instanceCreator = _root.GetCollection(schema.Id).CallFunction<MegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                        instanceCreator.Mode = WizardCreateMode.Batch;
                        var newId = MegaId.Create(instanceCreator.InvokeFunction<double>("Create"));
                        MegaObject item = _root.GetCollection(schema.Id).Item(newId);
                        switch(idType)
                        {
                            case IdTypeEnum.INTERNAL:
                                throw new ExecutionError("You cannot set the id in BUSINESS mode.");
                            case IdTypeEnum.EXTERNAL:
                                item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                break;
                        }
                        element = new HopexModelElement(this, schema, item);
                    }
                    else
                    {
                        IMegaObject item = null;
                        var coll = _iRoot.GetCollection(schema.Id);
                        if (!string.IsNullOrEmpty(id))
                        {
                            switch (idType)
                            {
                                case IdTypeEnum.INTERNAL:
                                    var megaId = MegaId.Create($"~{id}");
                                    item = coll.Create(megaId);
                                    break;
                                case IdTypeEnum.EXTERNAL:
                                    item = coll.Create();
                                    item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                    break;
                            }
                        }
                        else
                        {
                            item = coll.Create();
                        }
                        element = new HopexModelElement(this, schema, item);
                    }
                }
                //_logger.LogInformation("before update from create");
                await element.UpdateElement(settersList);
                //_logger.LogInformation("after update from create");
                return element;
            });
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
                throw new Exception($"Element {schema.Name} not found with id {id}");
            }
            var element = await getElementByIdAsyncTask;
            if (!(element is HopexModelElement))
            {
                throw new Exception($"Element {schema.Name} not found with id {id}");
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

            if(string.IsNullOrEmpty(id) && idType == IdTypeEnum.EXTERNAL)
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
                }
                if(obj != null && obj.Exists)
                {
                    return await UpdateElementAsync(schema, obj.MegaUnnamedField, IdTypeEnum.INTERNAL, setters);
                }
            }
            return await CreateElementAsync(schema, id, idType, useInstanceCreator, setters);
        }

        public Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, IdTypeEnum idType, bool cascade)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            MegaObject obj = null;
            switch (idType)
            {
                case IdTypeEnum.INTERNAL:
                    obj = _root.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{id}\"").FirstOrDefault();
                    break;
                case IdTypeEnum.EXTERNAL:
                    obj = _root.GetSelection($"SELECT {schema.Id}[{schema.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"").FirstOrDefault();
                    break;
            }

            if (obj != null)
            {
                var elementPermissions = CrudComputer.GetCrud(obj);
                if (! elementPermissions.IsDeletable)
                {
                    throw new ExecutionError($"You are not allowed to perform this action on this object ({obj.MegaField})");
                }

                return ProcessMutation("delete", obj.MegaField, () =>
                {
                    var options = cascade ? string.Empty : "NoHierarchy";
                    obj.Delete(options);
                    return Task.FromResult<IModelElement>(null);
                });
            }
            return Task.FromResult<IModelElement>(null);
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
    }
}
