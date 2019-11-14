using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Mega.Macro.API;
using Mega.Macro.API.Enums;
using Mega.Macro.API.Utils;

namespace Hopex.Model.DataModel
{
    internal class ClassCollectionDescription : IRelationshipDescription
    {
        public ClassCollectionDescription(IClassDescription schema)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            ClassDescription = schema;
            Name = schema.Name;
            RoleId = schema.Id;
            Path = new[] { new PathDescription(Name, RoleId) };
        }

        public string Name { get; }
        public MegaId RoleId { get; }
        public string Description { get; }
        public IPathDescription[] Path { get; }
        public IClassDescription ClassDescription { get; }
    }

    public class HopexDataModel : IHopexDataModel, IDisposable
    {
        private readonly MegaRoot _root;

        public IHopexMetaModel MetaModel { get; }

        public HopexDataModel(IHopexMetaModel schema, MegaRoot root)
        {
            MetaModel = schema;
            _root = root;
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

        public Task<IModelCollection> GetCollectionAsync(string name)
        {
            var schema = MetaModel.GetClassDescription(name);
            return Task.FromResult<IModelCollection>(new HopexModelCollection(this, new ClassCollectionDescription(schema), _root));
        }

        public async Task<IModelElement> CreateElementAsync(IClassDescription schema, IEnumerable<ISetter> setters, bool useInstanceCreator)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            MegaObject item;

            if (useInstanceCreator)
            {
                var instanceCreator = _root.GetCollection(schema.Id)
                    .CallFunction<MegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                instanceCreator.Mode = WizardCreateMode.Batch;
                var id = MegaId.Create(instanceCreator.InvokeFunction<double>("Create"));
                item = _root.GetCollection(schema.Id).Item(id);
            }
            else
            {
                var coll = _root.GetCollection(schema.Id);
                item = coll.Create();
            }

            var element = new HopexModelElement(this, schema, item);
            await element.UpdateElement(setters);
            return element;
        }

        public async Task<IModelElement> UpdateElementAsync(IClassDescription schema, string id, IEnumerable<ISetter> setters)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var element = await GetElementByIdAsync(schema, id);
            if (element == null)
            {
                throw new Exception($"Element {schema.Name} not found with id {id}");
            }

            await (element as HopexModelElement).UpdateElement(setters);
            return element;
        }

        public Task<IModelElement> RemoveElementAsync(IClassDescription schema, string id, bool cascade)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }
            var obj = _root.GetObjectFromId<MegaRoot>(Utils.NormalizeHopexId(id));
            if (obj != null)
            {
                var options = cascade ? string.Empty : "NoHierarchy";
                obj.Delete(options);
            }
            return Task.FromResult<IModelElement>(null);
        }

        public void Dispose()
        {
            (_root as IDisposable)?.Dispose();
        }
    }
}
