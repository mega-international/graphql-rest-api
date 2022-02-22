using GraphQL.Execution;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IModelContext
    {

    }

    public interface IModelElement : IHasCollection
    {
        MegaId Id { get; }
        MegaObject MegaObject { get; }
        IMegaObject IMegaObject { get; }
        IModelElement Parent { get; }
        IModelContext Context { get; }
        IHopexDataModel DomainModel { get; }
        IClassDescription ClassDescription { get; }
        IEnumerable<Exception> Errors { get; }
        T GetValue<T>(IPropertyDescription propertyDescription, IDictionary<string, ArgumentValue> arguments = null, string format = null);
        void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null);
        CrudResult GetCrud();
        CrudResult GetPropertyCrud(IPropertyDescription property);
        bool IsReadOnly(IPropertyDescription property);
        bool IsReadWrite(IPropertyDescription property);

        bool IsConfidential { get; }
        bool IsAvailable { get; }

        object GetGenericValue(string propertyMegaId, IDictionary<string, ArgumentValue> arguments);

        IModelCollection GetGenericCollection(string collectionMegaId);

        void AddErrors(IModelElement subElement);
        void CreateContext(IModelElement targetLinkAttributes, IEnumerable<IPropertyDescription> linkAttributes);
        void SpreadContextFromParent();
        IMegaObject Language { get; set; }

        IModelElement BuildChildElement(IMegaObject megaObject, IRelationshipDescription relationship, int pathIdx);
        Task<IModelElement> GetElementByIdAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType);
        Task<IModelElement> LinkElementAsync(IRelationshipDescription relationship, bool useInstanceCreator, IModelElement elementToLink, IEnumerable<ISetter> setters);
        Task<IModelElement> CreateElementAsync(IRelationshipDescription relationship, string id, IdTypeEnum idType, bool useInstanceCreator, IEnumerable<ISetter> setters);
        Task<IModelElement> UpdateAsync(IEnumerable<ISetter> setters);
    }
}
