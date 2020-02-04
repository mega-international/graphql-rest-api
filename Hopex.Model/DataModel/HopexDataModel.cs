using GraphQL;
using Hopex.ApplicationServer.WebServices;
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
    }

    public class HopexDataModel : IHopexDataModel, IDisposable
    {
        private readonly MegaRoot _root;
        private ILogger _logger;
        private static readonly Queue<CancellationTokenSource> _eventQueue = new Queue<CancellationTokenSource>();
        private static readonly object _eventLock = new object();

        public IHopexMetaModel MetaModel { get; }

        public HopexDataModel(IHopexMetaModel schema, MegaRoot root, ILogger logger)
        {
            MetaModel = schema;
            _root = root;
            _logger = logger;
        }

        public Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string id)
        {
            var obj = _root.GetObjectFromId<MegaObject>(Utils.NormalizeHopexId(id));
            if (obj == null)
            {
                return null;
            }

            return Task.FromResult<IModelElement>(new HopexModelElement(this, schema, obj, id));
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
            return Task.FromResult<IModelCollection>(new HopexModelCollection(this, new ClassCollectionDescription(id, schema), _root, erql, orderByClauses));
        }

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var permissions = _root.GetCollectionDescription(schema.Id).CallFunction<MegaWrapperObject>("~f8pQpjMDK1SP[GetMetaPermission]")?.NativeObject as string;
            var settersList = setters.ToList();
            if (permissions == null || !permissions.Contains("C") || settersList.Any() && !permissions.Contains("U"))
            {
                throw new ExecutionError("You are not allowed to perform this action");
            }

            return await ProcessMutation("create", null, async() =>
            {
                MegaObject item;
                if(Equals(schema.Id, "~UkPT)TNyFDK5") || Equals(schema.Id, "~aMRn)bUIGjX3"))
                {
                    var filePath = Path.GetTempPath() + "empty.txt";
                    File.WriteAllText(filePath, "");
                    var instanceCreator = _root.GetCollection(schema.Id).CallFunction<MegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                    instanceCreator.InvokePropertyPut("~vjh4n6oyFTKK[Localisation du fichier]", filePath);
                    var id = MegaId.Create(instanceCreator.InvokeFunction<double>("Create"));
                    item = _root.GetCollection(schema.Id).Item(id);
                }
                else
                {
                    if(useInstanceCreator)
                    {
                        var instanceCreator = _root.GetCollection(schema.Id).CallFunction<MegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                        instanceCreator.Mode = WizardCreateMode.Batch;
                        var id = MegaId.Create(instanceCreator.InvokeFunction<double>("Create"));
                        item = _root.GetCollection(schema.Id).Item(id);
                    }
                    else
                    {
                        var coll = _root.GetCollection(schema.Id);
                        item = coll.Create();
                    }
                }
                //_logger.LogInformation("before new hopexModelElement");
                var element = new HopexModelElement(this, schema, item);
                //_logger.LogInformation("before update from create");
                await element.UpdateElement(settersList);
                //_logger.LogInformation("after update from create");
                return element;
            });
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IEnumerable<ISetter> setters)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var getElementByIdAsyncTask = GetElementByIdAsync(schema, id);
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

            return await ProcessMutation("update", element.MegaObject.MegaField, async() =>
            {
                await (element as HopexModelElement).UpdateElement(setters);
                return element;
            });
        }

        public Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, bool cascade)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }
            var obj = _root.GetObjectFromId<MegaObject>(Utils.NormalizeHopexId(id));
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
            var success = false;
            try
            {
                //_logger.LogInformation("New mutation");
                var willSleep = false;
                var cts = new CancellationTokenSource();
                lock(_eventLock)
                {
                    _eventQueue.Enqueue(cts);
                    //_logger.LogInformation($"mutation count begin: {_eventQueue.Count}");
                    if(_eventQueue.Count > 1)
                    {
                        willSleep = true;
                    }
                }
                if(willSleep)
                {
                    //_logger.LogInformation("mutation sleep");
                    try
                    {
                        await Task.Delay(Timeout.Infinite, cts.Token);
                    }
                    catch(TaskCanceledException)
                    {
                        //_logger.LogInformation("mutation freed");
                    }
                }
                //_logger.LogInformation("mutation start");
                var result = await mutation.Invoke();
                success = true;
                return result;
            }
            finally
            {
                //_logger.LogInformation("mutation ended");
                if(success)
                {
                    PublishSession();
                }
                lock(_eventLock)
                {
                    _eventQueue.Dequeue();
                    //_logger.LogInformation($"mutation count end: {_eventQueue.Count}");
                    if(_eventQueue.Any())
                    {
                        var next = _eventQueue.Peek();
                        next.Cancel();
                        //_logger.LogInformation("next mutation awaken");
                    }
                }
            }
        }

        private void PublishSession()
        {
            var result = _root.CallFunction<MegaWrapperObject>("~lcE6jbH9G5cK", "{\"instruction\":\"PUBLISHINSESSION\"}")?.NativeObject as string;
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
