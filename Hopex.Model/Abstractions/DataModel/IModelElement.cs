using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using GraphQL.Execution;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IModelElement : IHasCollection
    {
        MegaId Id { get; }
        MegaObject MegaObject { get; }
        IMegaObject IMegaObject { get; }
        IMegaObject Parent { get; }
        IHopexDataModel DomainModel { get; }

        IClassDescription ClassDescription { get; }

        IEnumerable<Exception> Errors { get; }

        T GetValue<T>(string name, IDictionary<string, ArgumentValue> arguments = null, string format = null);
        void SetValue<T>(string name, T value, string format = null);

        T GetValue<T>(IPropertyDescription propertyDescription, IDictionary<string, ArgumentValue> arguments = null, string format = null);
        void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null);
        CrudResult GetCrud();
        CrudResult GetPropertyCrud(IPropertyDescription property);

        bool IsConfidential { get; }
        bool IsAvailable { get; }

        object GetGenericValue(string propertyMegaId, IDictionary<string, ArgumentValue> arguments);

        IModelCollection GetGenericCollection(string collectionMegaId);

        void AddErrors(IModelElement subElement);
        IMegaObject Language { get; set; }
        IModelElement PathElement { get; set; }
    }
}
