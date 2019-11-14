using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface IModelElement : IHasCollection
    {
        MegaId Id { get; }

        IClassDescription ClassDescription { get; }

        T GetValue<T>(string name, string format = null);
        void SetValue<T>(string name, T value, string format = null);

        T GetValue<T>(IPropertyDescription propertyDescription, string format = null);
        void SetValue<T>(IPropertyDescription propertyDescription, T value, string format = null);
    }
}
