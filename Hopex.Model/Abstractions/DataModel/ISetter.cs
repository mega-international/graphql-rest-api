using System.Threading.Tasks;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface ISetter
    {
        IPropertyDescription PropertyDescription { get; }
        object Value { get; }
        Task UpdateElementAsync(IHopexDataModel model, IModelElement element);
    }
}
