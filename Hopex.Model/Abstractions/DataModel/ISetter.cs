using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface ISetter
    {
        Task UpdateElementAsync(IHopexDataModel model, IModelElement element);
    }
}
