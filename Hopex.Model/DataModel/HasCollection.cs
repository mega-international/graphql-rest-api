using GraphQL;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Enums;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hopex.Model.DataModel
{
    public abstract class HasCollection : IHasCollection
    {
        protected readonly IMegaRoot _iRoot;

        public HasCollection(IMegaRoot iRoot)
        {
            _iRoot = iRoot;
        }

        public abstract Task<IModelCollection> GetCollectionAsync(string name, string relationshipName, GetCollectionArguments getCollectionArguments);

        protected async Task<IModelElement> GetElementByIdAsync(IClassDescription schema, string relationshipName, string id, IdTypeEnum idType, IHopexDataModel domainModel)
        {
            string erql = null;
            if(id == null)
            {
                return null;
            }

            switch(idType)
            {
                case IdTypeEnum.INTERNAL:
                    if(schema?.Id != null && schema is not GenericClassDescription)
                    {
                        erql = $"SELECT {schema.Id}[{schema.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{id}\"";
                    }
                    else
                    {
                        var mo = _iRoot.GetObjectFromId(id);
                        if(mo == null || !mo.Exists)
                        {
                            return null;
                        }
                        return domainModel.BuildElement(mo, schema);
                    }
                    break;
                case IdTypeEnum.EXTERNAL:
                    erql = $"SELECT {schema.Id}[{schema.Name}] WHERE ~CFmhlMxNT1iE[ExternalIdentifier] = \"{id}\"";
                    break;
                case IdTypeEnum.TEMPORARY:
                    if(domainModel.TemporaryMegaObjects.TryGetValue(id, out var tempElement))
                    {
                        erql = $"SELECT {schema.Id}[{schema.Name}] WHERE {MetaAttributeLibrary.AbsoluteIdentifier} = \"{tempElement.Id}\"";
                    }
                    else
                    {
                        return null;
                    }
                    break;
            }

            var arguments = new GetCollectionArguments
            {
                Erql = erql
            };

            var collection = await GetCollectionAsync(schema.Name, relationshipName, arguments);
            var element = collection.FirstOrDefault();

            if(element?.IMegaObject?.Exists != true)
            {
                return null;
            }
            return element;
        }

        protected Task<IModelElement> CreateSingleElementAsync(IClassDescription entity, bool useInstanceCreator, string id, IdTypeEnum idType, IMegaCollection collection, Func<IMegaObject, IModelElement> builder, Dictionary<string, IModelElement> temporaryElements, bool finalElement)
        {
            IModelElement element = null;
            if(Equals(entity.Id, "~UkPT)TNyFDK5") || Equals(entity.Id, "~aMRn)bUIGjX3"))
            {
                var filePath = Path.GetTempPath() + "empty.txt";
                File.WriteAllText(filePath, "");
                var instanceCreator = collection.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                instanceCreator.InvokePropertyPut("~vjh4n6oyFTKK[Localisation du fichier]", filePath);
                var newId = MegaId.Create(instanceCreator.Create());
                var item = collection.Item(newId);
                CheckObjectCreation(item);
                element = builder(item);
            }
            else
            {
                if(useInstanceCreator)
                {
                    var instanceCreator = collection.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                    instanceCreator.Mode = WizardCreateMode.Batch;
                    var newId = MegaId.Create(instanceCreator.Create());
                    IMegaObject item = collection.Item(newId);
                    CheckObjectCreation(item);
                    if(finalElement && !string.IsNullOrEmpty(id))
                    {
                        switch(idType)
                        {
                            case IdTypeEnum.INTERNAL:
                                throw new ExecutionError("You cannot set the id in BUSINESS mode.");
                            case IdTypeEnum.EXTERNAL:
                                item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                break;
                        }
                    }
                    element = builder(item);
                }
                else
                {
                    IMegaObject item = null;
                    if(finalElement && !string.IsNullOrEmpty(id))
                    {
                        switch(idType)
                        {
                            case IdTypeEnum.INTERNAL:
                                var megaId = MegaId.Create($"~{id}");
                                item = collection.Create(megaId);
                                CheckObjectCreation(item);
                                break;
                            case IdTypeEnum.EXTERNAL:
                                item = collection.Create();
                                CheckObjectCreation(item);
                                var externalIdentifierPermissions = new CrudResult(item.CallFunctionString("~R2mHVReGFP46[WFQuery]", "~CFmhlMxNT1iE[ExternalIdentifier]"));
                                if(!externalIdentifierPermissions.IsUpdatable)
                                {
                                    item.Delete();
                                    throw new ExecutionError($"You are not allowed to create {entity.Name} with id type EXTERNAL");
                                }
                                item.SetPropertyValue(MegaId.Create("~CFmhlMxNT1iE[ExternalIdentifier]"), id);
                                break;
                            case IdTypeEnum.TEMPORARY:
                                item = collection.Create();
                                CheckObjectCreation(item);
                                element = builder(item);
                                temporaryElements.Add(id, element);
                                break;
                        }
                    }
                    else
                    {
                        item = collection.Create();
                        CheckObjectCreation(item);
                    }
                    element ??= builder(item);
                }
            }
            return Task.FromResult(element);
        }

        private void CheckObjectCreation(object obj)
        {
            if(obj is MegaObject megaObj)
            {
                if(!megaObj.Exists)
                {
                    throw new ExecutionError($"Object {megaObj.Id?.ToString() ?? "null"} is empty, creation failed");
                }
            }
            else if(obj is IMegaObject iMegaObj)
            {
                if(!iMegaObj.Exists)
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
