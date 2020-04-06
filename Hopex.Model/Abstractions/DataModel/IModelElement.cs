using System.Collections.Generic;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.DataModel;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IModelElement : IHasCollection
    {
        MegaId Id { get; }
        MegaObject MegaObject { get; }
        IMegaObject IMegaObject { get; }

        IClassDescription ClassDescription { get; }

        T GetValue<T>(string name, Dictionary<string, object> arguments = null, string format = null);
        void SetValue<T>(string name, T value, string format = null);

        T GetValue<T>(IPropertyDescription propertyDescription, Dictionary<string, object> arguments = null, string format = null);
        void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null);
        CrudResult GetCrud();
        bool IsConfidential { get; }
        bool IsAvailable { get; }

        object GetGenericValue(string popertyMegaId, Dictionary<string, object> arguments);

        IModelCollection GetGenericCollection(string collectionMegaId);
    }
}
